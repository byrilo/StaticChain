using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChainBuilder : MonoBehaviour
{
    [SerializeField] private Transform contentParent;

    private void OnEnable() => Build();

    private void Build()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int chainId = GameManager.Instance.GetChainIdForPlayer(myId, 0);
        int totalRounds = GameManager.Instance.TotalRounds;

        for (int round = 0; round < totalRounds; round++)
        {
            var contentType = GameManager.GetContentType(round);

            if (contentType == ContentType.Phrase)
            {
                if (PhraseManager.Instance.TryGetPhrase(chainId, round, out var text))
                    AddText(text);
            }
            else
            {
                if (DrawingManager.Instance.TryGetDrawing(chainId, round, out var png))
                    AddImage(png);
            }
        }
    }

    private void AddText(string text)
    {
        var go = new GameObject("ResultText", typeof(RectTransform));
        go.transform.SetParent(contentParent, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 60);
    }

    private void AddImage(byte[] pngData)
    {
        var go = new GameObject("ResultImage", typeof(RectTransform));
        go.transform.SetParent(contentParent, false);

        var raw = go.AddComponent<RawImage>();
        var texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);
        raw.texture = texture;

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 300);
    }
}
