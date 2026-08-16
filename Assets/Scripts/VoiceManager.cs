using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VoiceManager : NetworkBehaviour
{
    public static VoiceManager Instance { get; private set; }

    // У RPC в Netcode жёсткий потолок ~64 КБ на сообщение — режем аудио на куски меньше этого.
    private const int ChunkSize = 16000;

    private readonly Dictionary<(int chainId, int round), byte[]> _voices = new();
    private readonly Dictionary<ulong, List<byte>> _serverIncoming = new();
    private readonly Dictionary<ulong, List<byte>> _clientIncoming = new();

    private void Awake() => Instance = this;

    public void SubmitVoice(byte[] wavData)
    {
        int totalChunks = Mathf.CeilToInt(wavData.Length / (float)ChunkSize);
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int length = Mathf.Min(ChunkSize, wavData.Length - offset);
            var chunk = new byte[length];
            Array.Copy(wavData, offset, chunk, 0, length);
            SubmitVoiceChunkRpc(chunk, i, totalChunks);
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitVoiceChunkRpc(byte[] chunk, int chunkIndex, int totalChunks, RpcParams rpcParams = default)
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

        var wavData = buffer.ToArray();
        _serverIncoming.Remove(senderId);

        int round = GameManager.Instance.CurrentRound;
        if (round < 0 || round >= GameManager.Instance.TotalRounds) return;
        if (GameManager.GetContentType(round) != ContentType.Voice) return;

        int chainId = GameManager.Instance.GetChainIdForPlayer(senderId, round);
        if (chainId < 0) return;

        bool alreadySubmitted = GameManager.Instance.HasSubmittedThisRound(chainId);

        var distorted = AudioDistortion.ApplyRandomDistortion(wavData);
        BroadcastVoice(chainId, round, senderId, distorted);

        if (!alreadySubmitted)
            GameManager.Instance.ReportSubmission(chainId);
    }

    private void BroadcastVoice(int chainId, int round, ulong fromClientId, byte[] wavData)
    {
        int totalChunks = Mathf.CeilToInt(wavData.Length / (float)ChunkSize);
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int length = Mathf.Min(ChunkSize, wavData.Length - offset);
            var chunk = new byte[length];
            Array.Copy(wavData, offset, chunk, 0, length);
            BroadcastVoiceChunkRpc(chainId, round, fromClientId, chunk, i, totalChunks);
        }
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void BroadcastVoiceChunkRpc(int chainId, int round, ulong fromClientId, byte[] chunk, int chunkIndex, int totalChunks)
    {
        if (!_clientIncoming.TryGetValue(fromClientId, out var buffer))
        {
            buffer = new List<byte>();
            _clientIncoming[fromClientId] = buffer;
        }

        if (chunkIndex == 0) buffer.Clear();
        buffer.AddRange(chunk);

        if (chunkIndex != totalChunks - 1) return;

        var wavData = buffer.ToArray();
        _clientIncoming.Remove(fromClientId);

        _voices[(chainId, round)] = wavData;
    }

    public bool TryGetVoice(int chainId, int round, out byte[] wavData) =>
        _voices.TryGetValue((chainId, round), out wavData);

    public bool TryGetVoiceDuration(int chainId, int round, out float seconds)
    {
        if (!TryGetVoice(chainId, round, out var wav)) { seconds = 0f; return false; }
        var (samples, channels, sampleRate) = WavUtility.Decode(wav);
        seconds = samples.Length / (float)(sampleRate * Mathf.Max(channels, 1));
        return true;
    }

    [Rpc(SendTo.Everyone)]
    public void ClearAllRpc()
    {
        _voices.Clear();
    }
}
