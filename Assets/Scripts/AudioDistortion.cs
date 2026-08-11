using UnityEngine;

// Порча звука перед пересылкой дальше по цепочке — "испорченный телефон".
public static class AudioDistortion
{
    public static byte[] ApplyRandomDistortion(byte[] wavData)
    {
        var (samples, channels, sampleRate) = WavUtility.Decode(wavData);

        float durationSeconds = samples.Length / (float)(sampleRate * Mathf.Max(channels, 1));

        // Число пропаданий растёт вместе с длиной сообщения, но остаётся в разумных
        // пределах — чтобы смысл сообщения не терялся полностью.
        int dropouts = Mathf.Clamp(Mathf.RoundToInt(durationSeconds / 2f), 1, 6);

        for (int i = 0; i < dropouts; i++)
        {
            int segLength = Mathf.RoundToInt(samples.Length * Random.Range(0.05f, 0.15f));
            segLength = Mathf.Min(segLength, samples.Length / 4); // не больше четверти за один раз
            int start = Random.Range(0, Mathf.Max(1, samples.Length - segLength));
            for (int j = start; j < start + segLength && j < samples.Length; j++)
                samples[j] = 0f;
        }

        return WavUtility.Encode(samples, channels, sampleRate);
    }
}
