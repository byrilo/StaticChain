using System.Text;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance { get; private set; }

    [SerializeField] private TMP_Text playerListText;

    private readonly NetworkList<PlayerInfo> _players = new NetworkList<PlayerInfo>();

    private void Awake()
    {
        Instance = this;
        if (playerListText != null) playerListText.text = "Players in room:";
    }

    public override void OnNetworkSpawn()
    {
        _players.OnListChanged += OnPlayersChanged;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        RefreshText();
    }

    public override void OnNetworkDespawn()
    {
        _players.OnListChanged -= OnPlayersChanged;
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId) => _players.Add(new PlayerInfo { ClientId = clientId });

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = _players.Count - 1; i >= 0; i--)
        {
            if (_players[i].ClientId == clientId) { _players.RemoveAt(i); break; }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetNicknameRpc(FixedString32Bytes nickname, RpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        for (int i = 0; i < _players.Count; i++)
        {
            if (_players[i].ClientId != senderId) continue;
            var info = _players[i];
            info.Nickname = nickname;
            _players[i] = info;
            return;
        }
    }

    public string GetNickname(ulong clientId)
    {
        foreach (var p in _players)
        {
            if (p.ClientId != clientId) continue;
            return p.Nickname.IsEmpty ? $"Player {clientId}" : p.Nickname.ToString();
        }
        return $"Player {clientId}";
    }

    private void OnPlayersChanged(NetworkListEvent<PlayerInfo> _) => RefreshText();

    private void RefreshText()
    {
        var sb = new StringBuilder("Players in room:\n");
        foreach (var p in _players)
            sb.AppendLine(p.Nickname.IsEmpty ? $"Player {p.ClientId}" : p.Nickname.ToString());
        playerListText.text = sb.ToString();
    }
}
