using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using TMPro;
using UnityEngine;

public enum ContentType { Phrase, Drawing }

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TMP_Text gameStatusText;

    private readonly NetworkVariable<int> _currentRound = new NetworkVariable<int>(-1);
    private readonly NetworkVariable<int> _totalRounds = new NetworkVariable<int>(0);
    private readonly NetworkList<ulong> _playerOrder = new NetworkList<ulong>();
    private readonly HashSet<int> _submittedChainsThisRound = new HashSet<int>();

    public int CurrentRound => _currentRound.Value;
    public int TotalRounds => _totalRounds.Value;
    public int PlayerCount => _playerOrder.Count;
    public bool GameStarted => _currentRound.Value >= 0;

    public static ContentType GetContentType(int round) => round % 2 == 0 ? ContentType.Phrase : ContentType.Drawing;

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        _currentRound.OnValueChanged += (_, _) => RefreshStatus();
        _totalRounds.OnValueChanged += (_, _) => RefreshStatus();
        RefreshStatus();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void StartGameRpc(int requestedRounds, RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
        if (_currentRound.Value >= 0) return;

        var clientIds = NetworkManager.Singleton.ConnectedClientsIds.OrderBy(id => id).ToList();
        _playerOrder.Clear();
        foreach (var id in clientIds) _playerOrder.Add(id);

        _totalRounds.Value = requestedRounds > 0 ? requestedRounds : clientIds.Count;
        _currentRound.Value = 0;
    }

    public ulong GetPlayerAtSlot(int slot) => _playerOrder[slot];

    public int GetMySlot(ulong clientId)
    {
        for (int i = 0; i < _playerOrder.Count; i++)
            if (_playerOrder[i] == clientId) return i;
        return -1;
    }

    public int GetChainIdForPlayer(ulong clientId, int round)
    {
        int slot = GetMySlot(clientId);
        if (slot < 0) return -1;
        int n = _playerOrder.Count;
        return ((slot - round) % n + n) % n;
    }

    public bool HasSubmittedThisRound(int chainId) => _submittedChainsThisRound.Contains(chainId);

    public void ReportSubmission(int chainId)
    {
        if (!IsServer) return;

        _submittedChainsThisRound.Add(chainId);
        if (_submittedChainsThisRound.Count >= _playerOrder.Count)
        {
            _submittedChainsThisRound.Clear();
            _currentRound.Value = _currentRound.Value + 1 >= _totalRounds.Value
                ? _totalRounds.Value
                : _currentRound.Value + 1;
        }
    }

    private void RefreshStatus()
    {
        if (gameStatusText == null) return;
        gameStatusText.text = _currentRound.Value < 0
            ? "Лобби, ждём старта"
            : $"Раунд {_currentRound.Value + 1} / {_totalRounds.Value}";
    }
}
