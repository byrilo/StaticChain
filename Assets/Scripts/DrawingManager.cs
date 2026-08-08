using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : NetworkBehaviour
{
    public static DrawingManager Instance { get; private set; }

    [SerializeField] private RawImage receivedDrawingDisplay;

    private readonly Dictionary<(int chainId, int round), byte[]> _drawings = new();

    private void Awake() => Instance = this;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitDrawingRpc(byte[] pngData, RpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        int round = GameManager.Instance.CurrentRound;

        if (round < 0 || round >= GameManager.Instance.TotalRounds) return;
        if (GameManager.GetContentType(round) != ContentType.Drawing) return;

        int chainId = GameManager.Instance.GetChainIdForPlayer(senderId, round);
        if (chainId < 0) return;

        // Разрешаем обновлять уже отправленный рисунок (кнопка-переключатель Save/Edit),
        // но засчитываем сдачу раунда только один раз.
        bool alreadySubmitted = GameManager.Instance.HasSubmittedThisRound(chainId);

        BroadcastDrawingRpc(chainId, round, senderId, pngData);

        if (!alreadySubmitted)
            GameManager.Instance.ReportSubmission(chainId);
    }

    [Rpc(SendTo.Everyone)]
    private void BroadcastDrawingRpc(int chainId, int round, ulong fromClientId, byte[] pngData)
    {
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
