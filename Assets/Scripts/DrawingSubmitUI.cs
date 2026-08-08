using UnityEngine;

public class DrawingSubmitUI : MonoBehaviour
{
    [SerializeField] private DrawingCanvas drawingCanvas;

    public void OnSubmitClicked()
    {
        var pngData = drawingCanvas.EncodePng();
        DrawingManager.Instance.SubmitDrawingRpc(pngData);
        drawingCanvas.Lock();
    }
}
