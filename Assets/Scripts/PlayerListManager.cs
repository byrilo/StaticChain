using System.Text;
using Unity.Netcode;
using TMPro;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerListText;

    private readonly NetworkList<PlayerInfo> _players = new NetworkList<PlayerInfo>();

    private void Awake()
    {
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

    private void OnPlayersChanged(NetworkListEvent<PlayerInfo> _) => RefreshText();

    private void RefreshText()
    {
        var sb = new StringBuilder("Players in room:\n");
        foreach (var p in _players) sb.AppendLine($"Player {p.ClientId}");
        playerListText.text = sb.ToString();
    }
}