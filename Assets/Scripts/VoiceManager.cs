using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VoiceManager : NetworkBehaviour
{
    public static VoiceManager Instance { get; private set; }

    // У RPC в Netcode жёсткий потолок ~64 КБ на сообщение — режем аудио на куски меньше этого.
    private const int ChunkSize = 16000;

    [SerializeField] private AudioSource receivedAudioSource;

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

        if (chunkIndex == totalChunks - 1)
        {
            var fullData = buffer.ToArray();
            _serverIncoming.Remove(senderId);

            var distorted = AudioDistortion.ApplyRandomDistortion(fullData);
            BroadcastVoice(senderId, distorted);
        }
    }

    private void BroadcastVoice(ulong fromClientId, byte[] wavData)
    {
        int totalChunks = Mathf.CeilToInt(wavData.Length / (float)ChunkSize);
        for (int i = 0; i < totalChunks; i++)
        {
            int offset = i * ChunkSize;
            int length = Mathf.Min(ChunkSize, wavData.Length - offset);
            var chunk = new byte[length];
            Array.Copy(wavData, offset, chunk, 0, length);
            BroadcastVoiceChunkRpc(fromClientId, chunk, i, totalChunks);
        }
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void BroadcastVoiceChunkRpc(ulong fromClientId, byte[] chunk, int chunkIndex, int totalChunks)
    {
        if (!_clientIncoming.TryGetValue(fromClientId, out var buffer))
        {
            buffer = new List<byte>();
            _clientIncoming[fromClientId] = buffer;
        }

        if (chunkIndex == 0) buffer.Clear();
        buffer.AddRange(chunk);

        if (chunkIndex != totalChunks - 1) return;

        var fullData = buffer.ToArray();
        _clientIncoming.Remove(fromClientId);

        if (receivedAudioSource != null)
        {
            var clip = WavUtility.ToAudioClip(fullData);
            receivedAudioSource.pitch = UnityEngine.Random.Range(0.75f, 1.4f);
            receivedAudioSource.clip = clip;
            receivedAudioSource.Play();
        }

        Debug.Log($"Получено голосовое от игрока {fromClientId}, {fullData.Length} байт");
    }
}
