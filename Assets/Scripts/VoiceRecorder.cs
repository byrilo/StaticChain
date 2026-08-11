using UnityEngine;

public class VoiceRecorder : MonoBehaviour
{
    private const int MaxDurationSeconds = 15;
    private const int SampleRate = 16000;

    private AudioClip _recordedClip;
    private string _micDevice;
    private bool _isRecording;

    public bool IsRecording => _isRecording;
    public bool HasRecording => _recordedClip != null;

    private void Awake()
    {
        if (Microphone.devices.Length > 0)
            _micDevice = Microphone.devices[0];
        else
            Debug.LogWarning("Микрофон не найден — запись голоса не будет работать.");
    }

    public void StartRecording()
    {
        if (string.IsNullOrEmpty(_micDevice)) return;
        _recordedClip = Microphone.Start(_micDevice, false, MaxDurationSeconds, SampleRate);
        _isRecording = true;
    }

    public void StopRecording()
    {
        if (!_isRecording) return;

        int position = Microphone.GetPosition(_micDevice);
        Microphone.End(_micDevice);
        _isRecording = false;

        if (position <= 0 || _recordedClip == null)
        {
            _recordedClip = null;
            return;
        }

        // Обрезаем клип до реально записанной длины (Microphone.Start резервирует буфер на MaxDurationSeconds).
        var samples = new float[position * _recordedClip.channels];
        _recordedClip.GetData(samples, 0);
        var trimmed = AudioClip.Create("Recording", position, _recordedClip.channels, _recordedClip.frequency, false);
        trimmed.SetData(samples, 0);
        _recordedClip = trimmed;
    }

    public byte[] EncodeToWav() => _recordedClip != null ? WavUtility.Encode(_recordedClip) : null;

    public void ClearRecording() => _recordedClip = null;
}
