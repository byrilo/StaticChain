using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private int textureSize = 512;
    [SerializeField] private int brushRadius = 4;

    private Texture2D _texture;
    private RawImage _rawImage;
    private Color _currentColor = Color.black;
    private Vector2? _lastPixelPos;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _texture = new Texture2D(textureSize, textureSize);
        _rawImage.texture = _texture;
        Clear();
    }

    public void SetColor(Color color) => _currentColor = color;

    public void SetColorBlack() => SetColor(Color.black);
    public void SetColorRed() => SetColor(Color.red);
    public void SetColorBlue() => SetColor(Color.blue);
    public void SetColorGreen() => SetColor(Color.green);

    public void Clear()
    {
        var pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        _texture.SetPixels(pixels);
        _texture.Apply();
        _lastPixelPos = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastPixelPos = null;
        DrawAt(eventData);
    }

    public void OnDrag(PointerEventData eventData) => DrawAt(eventData);

    public void OnPointerUp(PointerEventData eventData) => _lastPixelPos = null;

    private void DrawAt(PointerEventData eventData)
    {
        var rectTransform = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            return;

        var rect = rectTransform.rect;
        var uv = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height);

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;

        var pixelPos = new Vector2(uv.x * textureSize, uv.y * textureSize);

        if (_lastPixelPos.HasValue)
            DrawLine(_lastPixelPos.Value, pixelPos);
        else
            DrawCircle(pixelPos);

        _lastPixelPos = pixelPos;
        _texture.Apply();
    }

    private void DrawLine(Vector2 from, Vector2 to)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(from, to));
        steps = Mathf.Max(steps, 1);
        for (int i = 0; i <= steps; i++)
        {
            var point = Vector2.Lerp(from, to, i / (float)steps);
            DrawCircle(point);
        }
    }

    private void DrawCircle(Vector2 center)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);

        for (int x = -brushRadius; x <= brushRadius; x++)
        {
            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                if (x * x + y * y > brushRadius * brushRadius) continue;
                int px = cx + x;
                int py = cy + y;
                if (px < 0 || px >= textureSize || py < 0 || py >= textureSize) continue;
                _texture.SetPixel(px, py, _currentColor);
            }
        }
    }
}
