using TMPro;
using UnityEngine;

public class DrawingSubmitUI : MonoBehaviour
{
    [SerializeField] private DrawingCanvas drawingCanvas;
    [SerializeField] private TMP_Text buttonLabel;

    private bool _lastLocked;

    private void Update()
    {
        // Держим подпись кнопки в актуальном состоянии, даже если холст разблокировался
        // не из-за клика по этой кнопке (например, Clear() в начале нового раунда).
        if (drawingCanvas.IsLocked != _lastLocked)
        {
            _lastLocked = drawingCanvas.IsLocked;
            if (buttonLabel != null) buttonLabel.text = _lastLocked ? "Saved (tap to edit)" : "Save";
        }
    }

    public void OnSubmitClicked()
    {
        if (drawingCanvas.IsLocked)
        {
            drawingCanvas.Unlock();
        }
        else
        {
            var pngData = drawingCanvas.EncodePng();
            DrawingManager.Instance.SubmitDrawing(pngData);
            drawingCanvas.Lock();
        }
    }
}
