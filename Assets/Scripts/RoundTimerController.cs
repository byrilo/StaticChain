using Unity.Netcode;
using UnityEngine;
using TMPro;

public class RoundTimerController : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_InputField phraseInput;
    [SerializeField] private DrawingCanvas drawingCanvas;

    private const float PhraseTimeLimit = 30f;
    private const float DrawingTimeLimit = 180f;

    private int _lastRound = int.MinValue;
    private float _timeLeft;
    private bool _autoActionDone;

    private void Awake()
    {
        if (timerText != null) timerText.text = "";
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) return;

        int round = GameManager.Instance.CurrentRound;
        bool roundActive = round >= 0 && round < GameManager.Instance.TotalRounds;

        if (round != _lastRound)
        {
            _lastRound = round;
            _autoActionDone = false;
            _timeLeft = roundActive
                ? (GameManager.GetContentType(round) == ContentType.Phrase ? PhraseTimeLimit : DrawingTimeLimit)
                : 0f;
        }

        if (!roundActive)
        {
            if (timerText != null) timerText.text = "";
            return;
        }

        _timeLeft -= Time.deltaTime;
        if (_timeLeft < 0) _timeLeft = 0;

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(_timeLeft);
            int minutes = seconds / 60;
            timerText.text = minutes > 0 ? $"{minutes}:{seconds % 60:00}" : $"{seconds}s";
        }

        if (_timeLeft <= 0f && !_autoActionDone)
        {
            _autoActionDone = true;
            AutoSubmit(round);
        }
    }

    private void AutoSubmit(int round)
    {
        var contentType = GameManager.GetContentType(round);

        if (contentType == ContentType.Phrase)
        {
            var text = phraseInput != null ? phraseInput.text : "";
            if (string.IsNullOrWhiteSpace(text)) text = "...";
            PhraseManager.Instance.SubmitPhraseRpc(text);
        }
        else
        {
            if (drawingCanvas == null) return;
            var png = drawingCanvas.EncodePng();
            DrawingManager.Instance.SubmitDrawingRpc(png);
            drawingCanvas.Lock();
        }
    }
}
