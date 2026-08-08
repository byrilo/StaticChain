using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private int textureSize = 512;
    [SerializeField] private int brushRadius = 4;
    [SerializeField] private Image colorPreview;
    [SerializeField] private TMPro.TMP_Text brushSizeText;

    private static readonly Color LockedTint = new Color(0.6f, 0.6f, 0.6f, 1f);

    private Texture2D _texture;
    private RawImage _rawImage;
    private Color _currentColor = Color.black;
    private Vector2? _lastPixelPos;
    private bool _locked;

    public bool IsLocked => _locked;

    private void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _texture = new Texture2D(textureSize, textureSize);
        _rawImage.texture = _texture;
        Clear();
        SetColor(_currentColor);
        RefreshBrushSizeText();
    }

    public byte[] EncodePng() => _texture.EncodeToPNG();

    public void SetColor(Color color)
    {
        _currentColor = color;
        if (colorPreview != null) colorPreview.color = color;
    }

    public void SetColorBlack() => SetColor(Color.black);
    public void SetColorRed() => SetColor(Color.red);
    public void SetColorBlue() => SetColor(Color.blue);
    public void SetColorGreen() => SetColor(Color.green);

    public void SetBrushSize(float size)
    {
        brushRadius = Mathf.Max(1, Mathf.RoundToInt(size));
        RefreshBrushSizeText();
    }

    private void RefreshBrushSizeText()
    {
        if (brushSizeText != null) brushSizeText.text = $"Brush: {brushRadius}";
    }

    public void Lock()
    {
        _locked = true;
        if (_rawImage != null) _rawImage.color = LockedTint;
    }

    public void Unlock()
    {
        _locked = false;
        if (_rawImage != null) _rawImage.color = Color.white;
    }

    public void Clear()
    {
        var pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        _texture.SetPixels(pixels);
        _texture.Apply();
        _lastPixelPos = null;
        Unlock();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_locked) return;
        _lastPixelPos = null;
        DrawAt(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_locked) return;
        DrawAt(eventData);
    }

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
            DrawStamp(pixelPos);

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
            DrawStamp(point);
        }
    }

    // Овальный штамп кисти с мягким (сглаженным) краем вместо жёсткого пиксельного круга.
    private void DrawStamp(Vector2 center)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);

        int radiusX = brushRadius;
        int radiusY = Mathf.Max(1, Mathf.RoundToInt(brushRadius * 0.7f));

        for (int x = -radiusX; x <= radiusX; x++)
        {
            for (int y = -radiusY; y <= radiusY; y++)
            {
                float nx = x / (float)radiusX;
                float ny = y / (float)radiusY;
                float dist = nx * nx + ny * ny;
                if (dist > 1f) continue;

                int px = cx + x;
                int py = cy + y;
                if (px < 0 || px >= textureSize || py < 0 || py >= textureSize) continue;

                float alpha = Mathf.Clamp01((1f - dist) * 3f);
                var blended = Color.Lerp(_texture.GetPixel(px, py), _currentColor, alpha);
                _texture.SetPixel(px, py, blended);
            }
        }
    }
}
