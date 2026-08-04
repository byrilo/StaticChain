using UnityEngine;

public class UIStateController : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject phraseControls;
    [SerializeField] private GameObject drawingControls;
    [SerializeField] private GameObject resultsPanel;

    private int _lastRound = int.MinValue;
    private bool _lastStarted;
    private bool _initialized;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        bool started = GameManager.Instance.GameStarted;
        int round = GameManager.Instance.CurrentRound;

        if (_initialized && started == _lastStarted && round == _lastRound) return;
        _initialized = true;
        _lastStarted = started;
        _lastRound = round;

        bool finished = started && round >= GameManager.Instance.TotalRounds;

        lobbyPanel.SetActive(!started);
        gamePanel.SetActive(started && !finished);
        resultsPanel.SetActive(finished);

        if (!started || finished)
        {
            phraseControls.SetActive(false);
            drawingControls.SetActive(false);
            return;
        }

        bool roundActive = round >= 0 && round < GameManager.Instance.TotalRounds;
        var contentType = roundActive ? GameManager.GetContentType(round) : ContentType.Phrase;

        phraseControls.SetActive(roundActive && contentType == ContentType.Phrase);
        drawingControls.SetActive(roundActive && contentType == ContentType.Drawing);
    }
}
