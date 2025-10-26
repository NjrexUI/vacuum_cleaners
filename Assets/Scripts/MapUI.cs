using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MapUI : MonoBehaviour
{
    public static MapUI Instance;
    public GameObject mapPanel; // root panel to toggle
    public RectTransform iconPrefab; // small image prefab (RectTransform with Image)
    public Sprite roomIconSprite; // sprite for room background icon
    public Sprite regularSprite;   // icon overlays for type (optional)
    public Sprite bossSprite;
    public Sprite shopSprite;
    public Sprite secretSprite;

    // map bounds used to normalize positions
    private Vector2 min, max;
    private List<RectTransform> icons = new List<RectTransform>();

    private void Awake()
    {
        Instance = this;
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (mapPanel != null) mapPanel.SetActive(!mapPanel.activeSelf);
        }
    }

    public void BuildMap(List<RoomController> rooms)
    {
        if (mapPanel == null || iconPrefab == null) return;

        // Clear previous
        foreach (var i in icons) Destroy(i.gameObject);
        icons.Clear();

        // compute bounds in world space
        min = new Vector2(rooms.Min(r => r.transform.position.x), rooms.Min(r => r.transform.position.y));
        max = new Vector2(rooms.Max(r => r.transform.position.x), rooms.Max(r => r.transform.position.y));

        float width = Mathf.Max(1f, max.x - min.x);
        float height = Mathf.Max(1f, max.y - min.y);

        foreach (var room in rooms)
        {
            var icon = Instantiate(iconPrefab, mapPanel.transform);
            icon.GetComponent<Image>().sprite = roomIconSprite;

            // overlay type sprite as child image if needed
            var overlay = icon.transform.Find("Overlay")?.GetComponent<Image>();
            if (overlay != null)
            {
                switch (room.roomType)
                {
                    case RoomType.Boss: overlay.sprite = bossSprite; break;
                    case RoomType.Shop: overlay.sprite = shopSprite; break;
                    case RoomType.Secret: overlay.sprite = secretSprite; break;
                    default: overlay.sprite = regularSprite; break;
                }
            }

            // calculate normalized position 0..1
            Vector2 p = room.transform.position;
            float nx = (p.x - min.x) / width;
            float ny = (p.y - min.y) / height;

            // size of panel
            RectTransform panelRect = mapPanel.GetComponent<RectTransform>();
            Vector2 panelSize = panelRect.rect.size;

            // put icon
            icon.anchoredPosition = new Vector2(Mathf.Lerp(-panelSize.x / 2f, panelSize.x / 2f, nx),
                                                Mathf.Lerp(-panelSize.y / 2f, panelSize.y / 2f, ny));

            icons.Add(icon);
        }
    }
}
