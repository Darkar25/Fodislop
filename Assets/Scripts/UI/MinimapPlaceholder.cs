using UnityEngine;
using UnityEngine.UI;
using Fodinae.Assets.Scripts.World;
using Fodinae.Assets.Scripts.Game.Managers;
using Fodinae.Assets.Scripts.Player;
using MinesServer.Data;

namespace Fodinae.Assets.Scripts.UI
{
    public class MinimapPlaceholder : MonoBehaviour
    {
        private Text coordinatesText;

        void Start()
        {
            try
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObj = new GameObject("Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }

                // 1. Создаём мини-карту
                GameObject minimapObj = new GameObject("MinimapPlaceholder");
                minimapObj.transform.SetParent(canvas.transform, false);
                Image image = minimapObj.AddComponent<Image>();

                MapManager mapManager = MapManager.Instance;
                if (mapManager == null)
                {
                    Debug.LogError("[MinimapPlaceholder] MapManager not initialized!");
                    return;
                }
                int mapWidth = mapManager.WorldWidth;
                int mapHeight = mapManager.WorldHeight;
                if (mapWidth <= 0 || mapHeight <= 0)
                {
                    Debug.LogError("[MinimapPlaceholder] Invalid map dimensions: " + mapWidth + "x" + mapHeight);
                    return;
                }

                Texture2D tex = new Texture2D(mapWidth, mapHeight);
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        CellType cellType = (CellType)x;
                        Color cellColor = mapManager.GetCellMinimapColor(cellType);
                        if (cellColor == Color.clear)
                            cellColor = Color.gray;
                        tex.SetPixel(x, y, cellColor);
                    }
                }
                tex.Apply();
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, mapWidth, mapHeight), new Vector2(0.5f, 0.5f));
                image.sprite = sprite;
                image.preserveAspect = true;

                RectTransform rectTransform = minimapObj.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.anchoredPosition = new Vector2(10, 10);
                rectTransform.sizeDelta = new Vector2(250, 250);

                // 2. Создаём текст координат (НАД мини-картой)
                CreateCoordinatesText(canvas, minimapObj);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MinimapPlaceholder] Ошибка в Start: {e.Message}\n{e.StackTrace}");
            }
        }

        private void CreateCoordinatesText(Canvas canvas, GameObject minimapObj)
        {
            try
            {
                GameObject textObj = new GameObject("PlayerCoordinates");
                textObj.transform.SetParent(canvas.transform, false);
                coordinatesText = textObj.AddComponent<Text>();
                if (coordinatesText == null)
                {
                    Debug.LogError("[MinimapPlaceholder] Не удалось добавить Text компонент!");
                    return;
                }

                Font font = null;
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    if (font != null)
                        Debug.Log("[MinimapPlaceholder] Шрифт Arial загружен через Resources");
                }
                catch { }

                if (font == null)
                {
                    try
                    {
                        font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                        if (font != null)
                            Debug.Log("[MinimapPlaceholder] Создан динамический шрифт Arial");
                    }
                    catch { }
                }

                if (font == null)
                {
                    try
                    {
                        Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
                        if (fonts != null && fonts.Length > 0)
                        {
                            font = fonts[0];
                            Debug.Log($"[MinimapPlaceholder] Найден альтернативный шрифт: {font.name}");
                        }
                    }
                    catch { }
                }

                if (font == null)
                {
                    try
                    {
                        font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                    }
                    catch { }
                }

                coordinatesText.font = font;
                coordinatesText.fontSize = 18;
                coordinatesText.color = Color.white; // БЕЛЫЙ шрифт
                coordinatesText.alignment = TextAnchor.MiddleLeft;
                coordinatesText.text = "X: 000 | Y: 000";
                coordinatesText.raycastTarget = false;
                coordinatesText.fontStyle = FontStyle.Bold;
                coordinatesText.resizeTextForBestFit = false;

                // Добавляем тень для лучшей читаемости на светлом фоне
                Shadow shadow = textObj.AddComponent<Shadow>();
                shadow.effectColor = Color.black;
                shadow.effectDistance = new Vector2(1, -1);

                // Позиционируем текст НАД мини-картой
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                RectTransform minimapRect = minimapObj.GetComponent<RectTransform>();

                if (textRect != null && minimapRect != null)
                {
                    // Рассчитываем позицию: X совпадает с левым краем мини-карты, Y чуть выше
                    float minimapX = minimapRect.anchoredPosition.x;
                    float minimapY = minimapRect.anchoredPosition.y + minimapRect.sizeDelta.y;

                    textRect.anchorMin = new Vector2(0, 0);
                    textRect.anchorMax = new Vector2(0, 0);
                    textRect.pivot = new Vector2(0, 1); // pivot в левом верхнем углу
                    textRect.anchoredPosition = new Vector2(minimapX + 75, minimapY);
                    textRect.sizeDelta = new Vector2(200, 30);
                }

                textObj.transform.SetAsLastSibling();

                Debug.Log($"[MinimapPlaceholder] Текст координат успешно создан! Шрифт: {(font != null ? font.name : "null (используется дефолтный)")}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MinimapPlaceholder] Ошибка при создании текста: {e.Message}\n{e.StackTrace}");
            }
        }

        void Update()
        {
            if (coordinatesText == null)
            {
                if (Time.frameCount % 60 == 0)
                    Debug.LogWarning("[MinimapPlaceholder] coordinatesText == null в Update, текст не обновляется");
                return;
            }

            try
            {
                PlayerMovementController playerMovement = FindObjectOfType<PlayerMovementController>();
                if (playerMovement != null)
                {
                    Vector2Int pos = playerMovement.ClientPosition;
                    int real_y = pos.y;
                    coordinatesText.text = $"<size=30>{pos.x}:{real_y}</size>";
                }
            }
            catch (System.Exception e)
            {
                if (Time.frameCount % 120 == 0)
                    Debug.LogWarning($"[MinimapPlaceholder] Ошибка обновления: {e.Message}");
            }
        }
    }
}