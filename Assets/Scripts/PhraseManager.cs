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

        for (int i = 0; i < _phrases.Count; i++)
        {
            if (_phrases[i].ClientId == senderId)
            {
                _phrases[i] = new PhraseEntry { ClientId = senderId, Phrase = phrase };
                return;
            }
        }
        _phrases.Add(new PhraseEntry { ClientId = senderId, Phrase = phrase });
    }

    private void OnPhrasesChanged(NetworkListEvent<PhraseEntry> _) => RefreshText();

    private void RefreshText()
    {
        var sb = new StringBuilder("Фразы:\n");
        foreach (var p in _phrases) sb.AppendLine($"Игрок {p.ClientId}: {p.Phrase}");
        phraseStatusText.text = sb.ToString();
    }
}