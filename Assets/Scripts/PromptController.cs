using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptController : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private RawImage promptImage;
    [SerializeField] private DrawingCanvas drawingCanvas;

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

        if (contentType == ContentType.Phrase)
        {
            if (round == 0)
            {
                promptText.text = "Come up with your own phrase and send it";
            }
            else if (DrawingManager.Instance.TryGetDrawing(chainId, round - 1, out var png))
            {
                promptText.text = "Guess the phrase from the drawing:";
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
            promptText.text = PhraseManager.Instance.TryGetPhrase(chainId, round - 1, out var text)
                ? $"Draw: {text}"
                : "Waiting for the previous player's phrase...";
        }
    }
}
