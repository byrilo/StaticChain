using UnityEngine;
using UnityEngine.UI;
using TMPro;

// То, что реально показывается на экране каждого игрока во время финального показа —
// синхронизировано через RevealController.
public class RevealDisplayController : MonoBehaviour
{
    [SerializeField] private TMP_Text ownerNameText;
    [SerializeField] private TMP_Text pageIndicatorText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private RawImage contentImage;
    [SerializeField] private AudioSource voiceSource;

    private int _lastChain = int.MinValue;
    private int _lastEntry = int.MinValue;

    private void Update()
    {
        if (RevealController.Instance == null || !RevealController.Instance.IsRevealing) return;

        int chain = RevealController.Instance.ChainIndex;
        int entry = RevealController.Instance.EntryIndex;

        if (chain == _lastChain && entry == _lastEntry) return;
        _lastChain = chain;
        _lastEntry = entry;

        Refresh(chain, entry);
    }

    private void Refresh(int chainId, int round)
    {
        if (contentImage != null) contentImage.gameObject.SetActive(false);
        if (contentText != null) contentText.gameObject.SetActive(false);

        ulong ownerClientId = GameManager.Instance.GetPlayerAtSlot(chainId);
        string ownerName = PlayerListManager.Instance != null
            ? PlayerListManager.Instance.GetNickname(ownerClientId)
            : $"Player {ownerClientId}";

        if (ownerNameText != null) ownerNameText.text = $"{ownerName}'s chain";
        if (pageIndicatorText != null) pageIndicatorText.text = $"Page {round + 1} / {GameManager.Instance.TotalRounds}";

        var contentType = GameManager.GetContentType(round);

        if (contentType == ContentType.Voice)
        {
            if (VoiceManager.Instance != null && VoiceManager.Instance.TryGetVoice(chainId, round, out var wav))
            {
                if (voiceSource != null)
                {
                    var clip = WavUtility.ToAudioClip(wav);
                    voiceSource.pitch = Random.Range(0.85f, 1.25f);
                    voiceSource.clip = clip;
                    voiceSource.Play();
                }

                if (contentText != null)
                {
                    contentText.gameObject.SetActive(true);
                    contentText.text = "Listening...";
                }
            }
        }
        else
        {
            if (DrawingManager.Instance != null && DrawingManager.Instance.TryGetDrawing(chainId, round, out var png) && contentImage != null)
            {
                var texture = new Texture2D(2, 2);
                texture.LoadImage(png);
                contentImage.texture = texture;
                contentImage.gameObject.SetActive(true);
            }
        }
    }
}
