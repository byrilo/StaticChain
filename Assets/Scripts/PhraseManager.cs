using System.Text;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine;

public class PhraseManager : NetworkBehaviour
{
    public static PhraseManager Instance { get; private set; }

    [SerializeField] private TMP_Text phraseStatusText;

    private readonly NetworkList<PhraseEntry> _phrases = new NetworkList<PhraseEntry>();

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        _phrases.OnListChanged += OnPhrasesChanged;
        RefreshText();
    }

    public override void OnNetworkDespawn()
    {
        _phrases.OnListChanged -= OnPhrasesChanged;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitPhraseRpc(FixedString128Bytes phrase, RpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        int round = GameManager.Instance.CurrentRound;

        if (round < 0 || round >= GameManager.Instance.TotalRounds) return;
        if (GameManager.GetContentType(round) != ContentType.Phrase) return;

        int chainId = GameManager.Instance.GetChainIdForPlayer(senderId, round);
        if (chainId < 0) return;
        if (GameManager.Instance.HasSubmittedThisRound(chainId)) return;

        _phrases.Add(new PhraseEntry { ChainId = chainId, Round = round, ClientId = senderId, Phrase = phrase });
        GameManager.Instance.ReportSubmission(chainId);
    }

    public bool TryGetPhrase(int chainId, int round, out string text)
    {
        foreach (var p in _phrases)
        {
            if (p.ChainId == chainId && p.Round == round)
            {
                text = p.Phrase.ToString();
                return true;
            }
        }
        text = null;
        return false;
    }

    private void OnPhrasesChanged(NetworkListEvent<PhraseEntry> _) => RefreshText();

    private void RefreshText()
    {
        if (phraseStatusText == null) return;
        var sb = new StringBuilder("Фразы:\n");
        foreach (var p in _phrases) sb.AppendLine($"Цепь {p.ChainId}, раунд {p.Round}: {p.Phrase}");
        phraseStatusText.text = sb.ToString();
    }
}
