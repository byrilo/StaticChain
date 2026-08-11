using System.IO;
using System.Text;
using UnityEngine;

// Простой конвертер AudioClip <-> WAV-байты (16-bit PCM), без внешних библиотек.
public static class WavUtility
{
    public static byte[] Encode(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        return Encode(samples, clip.channels, clip.frequency);
    }

    public static byte[] Encode(float[] samples, int channels, int sampleRate)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        int byteRate = sampleRate * channels * 2;
        int dataSize = samples.Length * 2;

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)(channels * 2));
        writer.Write((short)16);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);

        foreach (var sample in samples)
        {
            short value = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
            writer.Write(value);
        }

        return stream.ToArray();
    }

    public static (float[] samples, int channels, int sampleRate) Decode(byte[] wavData)
    {
        using var stream = new MemoryStream(wavData);
        using var reader = new BinaryReader(stream);

        reader.ReadBytes(4); // "RIFF"
        reader.ReadInt32();
        reader.ReadBytes(4); // "WAVE"
        reader.ReadBytes(4); // "fmt "
        int fmtSize = reader.ReadInt32();
        reader.ReadInt16(); // audio format
        int channels = reader.ReadInt16();
        int sampleRate = reader.ReadInt32();
        reader.ReadInt32(); // byte rate
        reader.ReadInt16(); // block align
        reader.ReadInt16(); // bits per sample
        if (fmtSize > 16) reader.ReadBytes(fmtSize - 16);

        reader.ReadBytes(4); // "data"
        int dataSize = reader.ReadInt32();

        int sampleCount = dataSize / 2;
        var samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
            samples[i] = reader.ReadInt16() / (float)short.MaxValue;

        return (samples, channels, sampleRate);
    }

    public static AudioClip ToAudioClip(byte[] wavData, string name = "VoiceClip")
    {
        var (samples, channels, sampleRate) = Decode(wavData);
        int lengthSamples = channels > 0 ? samples.Length / channels : samples.Length;
        var clip = AudioClip.Create(name, Mathf.Max(lengthSamples, 1), Mathf.Max(channels, 1), sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
