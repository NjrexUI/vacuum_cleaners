using UnityEngine;
using UnityEngine.Tilemaps;

public class PaintManager : MonoBehaviour
{
    private Tilemap tilemap;
    public TileBase paintTile; // Assign any test tile here (e.g. a red tile)

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void PaintAt(Vector2 worldPos)
    {
        if (tilemap == null)
        {
            Debug.LogWarning("❌ No tilemap found!");
            return;
        }

        Vector3Int cellPos = tilemap.WorldToCell(worldPos);
        TileBase oldTile = tilemap.GetTile(cellPos);

        if (oldTile == null)
        {
            Debug.Log($"❌ No tile at {cellPos}");
            return;
        }

        Debug.Log($"🎨 Painting tile at {cellPos}");
        tilemap.SetTile(cellPos, paintTile); // Replace tile visually
    }
}
