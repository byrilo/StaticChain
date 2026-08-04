using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DrawingManager : NetworkBehaviour
{
    public static DrawingManager Instance { get; private set; }

    [SerializeField] private RawImage receivedDrawingDisplay;

    private void Awake() => Instance = this;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitDrawingRpc(byte[] pngData, RpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        BroadcastDrawingRpc(senderId, pngData);
    }

    [Rpc(SendTo.Everyone)]
    private void BroadcastDrawingRpc(ulong fromClientId, byte[] pngData)
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(pngData);

        if (receivedDrawingDisplay != null)
            receivedDrawingDisplay.texture = texture;

        Debug.Log($"Получен рисунок от игрока {fromClientId}, размер {pngData.Length} байт");
    }
}
