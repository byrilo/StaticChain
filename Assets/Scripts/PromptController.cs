using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptController : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private RawImage promptImage;
    [SerializeField] private DrawingCanvas drawingCanvas;
    [SerializeField] private VoiceRecorder voiceRecorder;
    [SerializeField] private AudioSource voicePlaybackSource;

    private int _lastRound = int.MinValue;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) return;

        if (GameManager.Instance.CurrentRound != _lastRound)
            Refresh();
    }

    private void Refresh()
    {
        int round = GameManager.Instance.CurrentRound;
        _lastRound = round;
        promptImage.gameObject.SetActive(false);

        if (round < 0)
        {
            promptText.text = "Waiting for the game to start";
            return;
        }

        if (round >= GameManager.Instance.TotalRounds)
        {
            promptText.text = "Rounds finished!";
            return;
        }

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int chainId = GameManager.Instance.GetChainIdForPlayer(myId, round);
        var contentType = GameManager.GetContentType(round);

        if (contentType == ContentType.Voice)
        {
            if (voiceRecorder != null) voiceRecorder.ClearRecording();

            if (round == 0)
            {
                promptText.text = "Record your own voice message and send it";
            }
            else if (DrawingManager.Instance.TryGetDrawing(chainId, round - 1, out var png))
            {
                promptText.text = "Record a voice message guessing what this is:";
                var texture = new Texture2D(2, 2);
                texture.LoadImage(png);
                promptImage.texture = texture;
                promptImage.gameObject.SetActive(true);
            }
            else
            {
                promptText.text = "Waiting for the previous player's drawing...";
            }
        }
        else
        {
            drawingCanvas.Clear();

            if (VoiceManager.Instance.TryGetVoice(chainId, round - 1, out var wav))
            {
                promptText.text = "Listen and draw what you hear (Replay to listen again)";
                PlayVoice(wav);
            }
            else
            {
                promptText.text = "Waiting for the previous player's voice message...";
            }
        }
    }

    // Вызывается кнопкой Replay в DrawingControls.
    public void OnReplayClicked()
    {
        int round = GameManager.Instance.CurrentRound;
        if (round <= 0 || GameManager.GetContentType(round) != ContentType.Drawing) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int chainId = GameManager.Instance.GetChainIdForPlayer(myId, round);
        if (VoiceManager.Instance.TryGetVoice(chainId, round - 1, out var wav))
            PlayVoice(wav);
    }

    private void PlayVoice(byte[] wavData)
    {
        if (voicePlaybackSource == null) return;
        var clip = WavUtility.ToAudioClip(wavData);
        voicePlaybackSource.pitch = Random.Range(0.75f, 1.4f);
        voicePlaybackSource.clip = clip;
        voicePlaybackSource.Play();
    }
}
