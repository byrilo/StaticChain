using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : NetworkBehaviour
{
    public static DrawingManager Instance { get; private set; }

    // У RPC в Netcode жёсткий потолок ~64 КБ на сообщение — режем PNG на куски меньше этого
    // (на случай очень детального рисунка).
    private const int ChunkSize = 16000;

    [SerializeField] private RawImage receivedDrawingDisplay;

    private readonly Dictionary<(int chainId, int round), byte[]> _drawings = new();
    private readonly Dictionary<ulong, List<byte>> _serverIncoming = new();
    private readonly Dictionary<ulong, List<byte>> _clientIncoming = new();

    private void Awake() => Instance = this;

    public void SubmitDrawing(byte[] pngData)
    {
        int totalChunks = Mathf.CeilToInt(pngData.Length / (float)ChunkSize);
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int length = Mathf.Min(ChunkSize, pngData.Length - offset);
            var chunk = new byte[length];
            Array.Copy(pngData, offset, chunk, 0, length);
            SubmitDrawingChunkRpc(chunk, i, totalChunks);
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitDrawingChunkRpc(byte[] chunk, int chunkIndex, int totalChunks, RpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;

        if (!_serverIncoming.TryGetValue(senderId, out var buffer))
        {
            buffer = new List<byte>();
            _serverIncoming[senderId] = buffer;
        }

        if (chunkIndex == 0) buffer.Clear();
        buffer.AddRange(chunk);

        if (chunkIndex != totalChunks - 1) return;

        var pngData = buffer.ToArray();
        _serverIncoming.Remove(senderId);

        int round = GameManager.Instance.CurrentRound;
        if (round < 0 || round >= GameManager.Instance.TotalRounds) return;
        if (GameManager.GetContentType(round) != ContentType.Drawing) return;

        int chainId = GameManager.Instance.GetChainIdForPlayer(senderId, round);
        if (chainId < 0) return;

        // Разрешаем обновлять уже отправленный рисунок (кнопка-переключатель Save/Edit),
        // но засчитываем сдачу раунда только один раз.
        bool alreadySubmitted = GameManager.Instance.HasSubmittedThisRound(chainId);

        BroadcastDrawing(chainId, round, senderId, pngData);

        if (!alreadySubmitted)
            GameManager.Instance.ReportSubmission(chainId);
    }

    private void BroadcastDrawing(int chainId, int round, ulong fromClientId, byte[] pngData)
    {
        int totalChunks = Mathf.CeilToInt(pngData.Length / (float)ChunkSize);
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int length = Mathf.Min(ChunkSize, pngData.Length - offset);
            var chunk = new byte[length];
            Array.Copy(pngData, offset, chunk, 0, length);
            BroadcastDrawingChunkRpc(chainId, round, fromClientId, chunk, i, totalChunks);
        }
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void BroadcastDrawingChunkRpc(int chainId, int round, ulong fromClientId, byte[] chunk, int chunkIndex, int totalChunks)
    {
        if (!_clientIncoming.TryGetValue(fromClientId, out var buffer))
        {
            buffer = new List<byte>();
            _clientIncoming[fromClientId] = buffer;
        }

        if (chunkIndex == 0) buffer.Clear();
        buffer.AddRange(chunk);

        if (chunkIndex != totalChunks - 1) return;

        var pngData = buffer.ToArray();
        _clientIncoming.Remove(fromClientId);

        _drawings[(chainId, round)] = pngData;

        if (receivedDrawingDisplay != null)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(pngData);
            receivedDrawingDisplay.texture = texture;
        }
    }

    public bool TryGetDrawing(int chainId, int round, out byte[] pngData) =>
        _drawings.TryGetValue((chainId, round), out pngData);

    [Rpc(SendTo.Everyone)]
    public void ClearAllRpc()
    {
        _drawings.Clear();
    }
}
