using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the current frames‑per‑second in the top‑left corner of the screen.
/// Attach this component to a GameObject that has a Canvas (or create a new Canvas
/// automatically if none exists). The script creates a UI Text element, updates it
/// each frame and formats the value with one decimal place.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    private const int SampleSize = 30; // number of frames to average
    private readonly float[] _frameTimes = new float[SampleSize];
    private int _frameIndex;
    private float _accumulatedTime;

    private Text _fpsText;

    private void Awake()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("FPSCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        GameObject textGO = new GameObject("FPSLabel");
        textGO.transform.SetParent(canvas.transform, false);
        _fpsText = textGO.AddComponent<Text>();
        _fpsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        _fpsText.fontSize = 14;
        _fpsText.alignment = TextAnchor.UpperLeft;
        _fpsText.color = Color.white;
        _fpsText.raycastTarget = false;
        RectTransform rt = _fpsText.rectTransform;
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);
    }

    private void Update()
    {
        _frameTimes[_frameIndex] = Time.unscaledDeltaTime;
        _frameIndex = (_frameIndex + 1) % SampleSize;
        _accumulatedTime = 0f;
        for (int i = 0; i < SampleSize; i++)
        {
            _accumulatedTime += _frameTimes[i];
        }
        float avgDelta = _accumulatedTime / SampleSize;
        float fps = avgDelta > 0f ? 1f / avgDelta : 0f;
        _fpsText.text = $"FPS: {fps:F1}";
    }
}