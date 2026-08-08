using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ColorWheelPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private DrawingCanvas drawingCanvas;
    [SerializeField] private int squareSize = 256;

    private RawImage _rawImage;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        GenerateSquare();
    }

    private void GenerateSquare()
    {
        var texture = new Texture2D(squareSize, squareSize, TextureFormat.RGBA32, false);

        // X — оттенок (hue) 0..360, Y — насыщенность (saturation) 0..1, яркость фиксирована.
        for (int y = 0; y < squareSize; y++)
        {
            for (int x = 0; x < squareSize; x++)
            {
                float hue = x / (float)(squareSize - 1);
                float saturation = y / (float)(squareSize - 1);
                var color = Color.HSVToRGB(hue, saturation, 1f);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        _rawImage.texture = texture;
    }

    public void OnPointerDown(PointerEventData eventData) => PickColor(eventData);
    public void OnDrag(PointerEventData eventData) => PickColor(eventData);

    private void PickColor(PointerEventData eventData)
    {
        var rectTransform = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            return;

        var rect = rectTransform.rect;
        float hue = Mathf.Clamp01((localPoint.x - rect.x) / rect.width);
        float saturation = Mathf.Clamp01((localPoint.y - rect.y) / rect.height);

        var color = Color.HSVToRGB(hue, saturation, 1f);
        drawingCanvas.SetColor(color);
    }
}
