using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptController : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private RawImage promptImage;

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
            promptText.text = "Ждём начала игры";
            return;
        }

        if (round >= GameManager.Instance.TotalRounds)
        {
            promptText.text = "Раунды закончились!";
            return;
        }

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int chainId = GameManager.Instance.GetChainIdForPlayer(myId, round);
        var contentType = GameManager.GetContentType(round);

        if (contentType == ContentType.Phrase)
        {
            if (round == 0)
            {
                promptText.text = "Придумайте свою фразу и отправьте";
            }
            else if (DrawingManager.Instance.TryGetDrawing(chainId, round - 1, out var png))
            {
                promptText.text = "Угадайте фразу по рисунку:";
                var texture = new Texture2D(2, 2);
                texture.LoadImage(png);
                promptImage.texture = texture;
                promptImage.gameObject.SetActive(true);
            }
            else
            {
                promptText.text = "Ждём рисунок предыдущего игрока...";
            }
        }
        else
        {
            promptText.text = PhraseManager.Instance.TryGetPhrase(chainId, round - 1, out var text)
                ? $"Нарисуйте: {text}"
                : "Ждём фразу предыдущего игрока...";
        }
    }
}
