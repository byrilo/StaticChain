using TMPro;
using UnityEngine;

public class VoiceSubmitUI : MonoBehaviour
{
    [SerializeField] private VoiceRecorder recorder;
    [SerializeField] private TMP_Text recordButtonLabel;
    [SerializeField] private TMP_Text statusText;

    public void OnRecordClicked()
    {
        if (recorder.IsRecording)
        {
            recorder.StopRecording();
            if (recordButtonLabel != null) recordButtonLabel.text = "Record";
            if (statusText != null)
                statusText.text = recorder.HasRecording ? "Recorded! Press Send." : "No microphone / empty recording.";
        }
        else
        {
            recorder.StartRecording();
            if (recordButtonLabel != null) recordButtonLabel.text = "Stop";
            if (statusText != null) statusText.text = "Recording...";
        }
    }

    public void OnSendClicked()
    {
        var wav = recorder.EncodeToWav();
        if (wav == null)
        {
            if (statusText != null) statusText.text = "Nothing recorded yet.";
            return;
        }

        VoiceManager.Instance.SubmitVoice(wav);
        if (statusText != null) statusText.text = "Sent!";
    }
}
