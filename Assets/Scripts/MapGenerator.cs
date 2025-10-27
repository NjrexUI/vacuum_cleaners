using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MapGenerator : MonoBehaviour
{
    [Header("Player Spawn")]
    public GameObject playerPrefab;   
    private GameObject playerInstance;

    [Header("Grid")]
    public int gridWidth = 11;       
    public int gridHeight = 9;
    public int startX = 5;         
    public int startY = 4;

    [Header("Room counts")]
    public int minRooms = 8;
    public int maxRooms = 14;

    [Header("Prefabs")]
    public GameObject roomPrefabRegular; 
    public GameObject roomPrefabBoss;
    public GameObject roomPrefabShop;
    public GameObject roomPrefabSecret;

    [Header("Spacing")]
    public float roomSpacingX = 16f; 
    public float roomSpacingY = 9f;

    // Internal
    private bool[,] occupied; 
    private List<Vector2Int> cells; 
    private Dictionary<Vector2Int, RoomController> roomLookup = new();
    
    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        occupied = new bool[gridWidth, gridHeight];
        cells = new List<Vector2Int>();
        roomLookup.Clear();

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Vector2Int start = new Vector2Int(startX, startY);
        q.Enqueue(start);
        occupied[start.x, start.y] = true;
        cells.Add(start);

        System.Random rnd = new System.Random();

        while (q.Count > 0 && cells.Count < maxRooms)
        {
            var curr = q.Dequeue();

            var dirs = new List<Vector2Int> {
                new Vector2Int(1,0),
                new Vector2Int(-1,0),
                new Vector2Int(0,1),
                new Vector2Int(0,-1)
            }.OrderBy(_ => rnd.Next()).ToList();

            foreach (var d in dirs)
            {
                if (cells.Count >= maxRooms) break;
                var next = curr + d;
                if (next.x < 0 || next.x >= gridWidth || next.y < 0 || next.y >= gridHeight) continue;
                if (occupied[next.x, next.y]) continue;

                if (rnd.NextDouble() < 0.6) 
                {
                    occupied[next.x, next.y] = true;
                    cells.Add(next);
                    q.Enqueue(next);
                }
            }
        }

        var candidateNeighbours = new List<Vector2Int>();
        while (cells.Count < minRooms)
        {
            candidateNeighbours.Clear();
            foreach (var c in cells)
            {
                var potential = new Vector2Int[] { c + Vector2Int.up, c + Vector2Int.down, c + Vector2Int.left, c + Vector2Int.right };
                foreach (var p in potential)
                {
                    if (p.x < 0 || p.x >= gridWidth || p.y < 0 || p.y >= gridHeight) continue;
                    if (!occupied[p.x, p.y]) candidateNeighbours.Add(p);
                }
            }

            if (candidateNeighbours.Count == 0) break;
            var pick = candidateNeighbours[rnd.Next(candidateNeighbours.Count)];
            occupied[pick.x, pick.y] = true;
            cells.Add(pick);
        }

        foreach (var cell in cells)
        {
            Vector2 world = GridToWorld(cell);
            var prefab = roomPrefabRegular;
            var go = Instantiate(prefab, world, Quaternion.identity, transform);
            var rc = go.GetComponent<RoomController>();
            if (rc == null) rc = go.AddComponent<RoomController>();
            rc.gridPosition = cell;
            rc.roomType = RoomType.Regular;
            roomLookup[cell] = rc;
        }

        var distances = new Dictionary<Vector2Int, int>();
        var queue = new Queue<Vector2Int>();
        distances[start] = 0;
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            var neighbours = new Vector2Int[] { p + Vector2Int.up, p + Vector2Int.down, p + Vector2Int.left, p + Vector2Int.right };
            foreach (var n in neighbours)
            {
                if (!roomLookup.ContainsKey(n)) continue;
                if (distances.ContainsKey(n)) continue;
                distances[n] = distances[p] + 1;
                queue.Enqueue(n);
            }
        }

        foreach (var kv in roomLookup)
        {
            var rc = kv.Value;
            if (distances.TryGetValue(kv.Key, out int d))
                rc.graphDistance = d;
            else
                rc.graphDistance = int.MaxValue;
        }

        int maxDist = distances.Values.Max();
        var furthest = distances.Where(kv => kv.Value == maxDist).Select(kv => kv.Key).ToList();
        var bossPos = furthest[rnd.Next(furthest.Count)];
        roomLookup[bossPos].roomType = RoomType.Boss;

        var remaining = roomLookup.Keys.Where(k => k != bossPos).ToList();

        var endRooms = roomLookup.Keys.Where(k =>
        {
            int neigh = 0;
            var p = k;
            if (roomLookup.ContainsKey(p + Vector2Int.up)) neigh++;
            if (roomLookup.ContainsKey(p + Vector2Int.down)) neigh++;
            if (roomLookup.ContainsKey(p + Vector2Int.left)) neigh++;
            if (roomLookup.ContainsKey(p + Vector2Int.right)) neigh++;
            return neigh == 1;
        }).Where(k => k != bossPos).ToList();

        Vector2Int shopPos;
        if (endRooms.Count > 0)
            shopPos = endRooms[rnd.Next(endRooms.Count)];
        else
            shopPos = remaining[rnd.Next(remaining.Count)];
        roomLookup[shopPos].roomType = RoomType.Shop;

        var secretCandidates = roomLookup.Keys.Where(k => k != bossPos && k != shopPos).ToList();
        var secretPos = secretCandidates[rnd.Next(secretCandidates.Count)];
        roomLookup[secretPos].roomType = RoomType.Secret;

        foreach (var kv in roomLookup.ToList())
        {
            var pos = kv.Key;
            var rc = kv.Value;

            switch (rc.roomType)
            {
                case RoomType.Boss:
                    ReplaceRoomPrefab(rc, roomPrefabBoss);
                    break;
                case RoomType.Shop:
                    ReplaceRoomPrefab(rc, roomPrefabShop);
                    break;
                case RoomType.Secret:
                    ReplaceRoomPrefab(rc, roomPrefabSecret);
                    break;
            }
        }

        foreach (var kv in roomLookup)
        {
            var pos = kv.Key;
            var rc = kv.Value;
            bool up = roomLookup.ContainsKey(pos + Vector2Int.up);
            bool down = roomLookup.ContainsKey(pos + Vector2Int.down);
            bool left = roomLookup.ContainsKey(pos + Vector2Int.left);
            bool right = roomLookup.ContainsKey(pos + Vector2Int.right);

            rc.SetDoorState(up, down, left, right);
        }

        foreach (var kv in roomLookup.ToList())
        {
            Vector2Int pos = kv.Key;
            RoomController rc = kv.Value;

            if (roomLookup.ContainsKey(pos + Vector2Int.up))
            {
                var neighbor = roomLookup[pos + Vector2Int.up];
                if (neighbor.doorPlaceDown != null)
                {
                    var doorChild = neighbor.doorPlaceDown.Find("Door");
                    if (doorChild != null)
                    {
                        var dt = doorChild.GetComponent<DoorTrigger>() ?? doorChild.gameObject.AddComponent<DoorTrigger>();
                        dt.targetRoom = neighbor;
                    }
                }
            }

            if (roomLookup.ContainsKey(pos + Vector2Int.down))
            {
                var neighbor = roomLookup[pos + Vector2Int.down];
                if (neighbor.doorPlaceUp != null)
                {
                    var doorChild = neighbor.doorPlaceUp.Find("Door");
                    if (doorChild != null)
                    {
                        var dt = doorChild.GetComponent<DoorTrigger>() ?? doorChild.gameObject.AddComponent<DoorTrigger>();
                        dt.targetRoom = neighbor;
                    }
                }
            }

            if (roomLookup.ContainsKey(pos + Vector2Int.left))
            {
                var neighbor = roomLookup[pos + Vector2Int.left];
                if (neighbor.doorPlaceRight != null)
                {
                    var doorChild = neighbor.doorPlaceRight.Find("Door");
                    if (doorChild != null)
                    {
                        var dt = doorChild.GetComponent<DoorTrigger>() ?? doorChild.gameObject.AddComponent<DoorTrigger>();
                        dt.targetRoom = neighbor;
                    }
                }
            }

            if (roomLookup.ContainsKey(pos + Vector2Int.right))
            {
                var neighbor = roomLookup[pos + Vector2Int.right];
                if (neighbor.doorPlaceLeft != null)
                {
                    var doorChild = neighbor.doorPlaceLeft.Find("Door");
                    if (doorChild != null)
                    {
                        var dt = doorChild.GetComponent<DoorTrigger>() ?? doorChild.gameObject.AddComponent<DoorTrigger>();
                        dt.targetRoom = neighbor;
                    }
                }
            }
        }


        RoomManager.Instance.RegisterGeneratedRooms(roomLookup.Values.ToList());

        Vector2 startWorld = GridToWorld(start);

        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        startWorld += new Vector2(0.2f, -0.2f); 

        playerInstance = Instantiate(playerPrefab, startWorld, Quaternion.identity);

        playerInstance.transform.position += Vector3.up * 0.5f;


        Debug.Log($"Spawned player at world {startWorld}");

        Debug.Log($"Spawn grid: ({startX}, {startY})   Grid size: {gridWidth}x{gridHeight}");

    }

    Vector2 GridToWorld(Vector2Int grid)
    {
        float centerOffsetX = (gridWidth - 1) / 2f;
        float centerOffsetY = (gridHeight - 1) / 2f;

        float wx = (grid.x - centerOffsetX) * roomSpacingX;
        float wy = (grid.y - centerOffsetY) * roomSpacingY;
        return new Vector2(wx, wy);
    }

    void ReplaceRoomPrefab(RoomController rc, GameObject newPrefab)
    {
        if (newPrefab == null) return;
        var grid = rc.gridPosition;
        var pos = rc.transform.position;
        Destroy(rc.gameObject);

        var go = Instantiate(newPrefab, pos, Quaternion.identity, transform);
        var newRc = go.GetComponent<RoomController>();
        if (newRc == null) newRc = go.AddComponent<RoomController>();
        newRc.gridPosition = grid;
        newRc.roomType = DetermineRoomTypeFromPrefab(newPrefab);
        roomLookup[grid] = newRc;
    }

    RoomType DetermineRoomTypeFromPrefab(GameObject prefab)
    {
        if (prefab == roomPrefabBoss) return RoomType.Boss;
        if (prefab == roomPrefabShop) return RoomType.Shop;
        if (prefab == roomPrefabSecret) return RoomType.Secret;
        return RoomType.Regular;
    }
}
