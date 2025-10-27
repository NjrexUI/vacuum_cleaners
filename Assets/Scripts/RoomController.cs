using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomController : MonoBehaviour
{
    public Vector2Int gridPosition;
    public RoomType roomType = RoomType.Regular;
    public int graphDistance = 0;

    [Header("Door & Wall Anchors")]
    public Transform doorPlaceUp;
    public Transform doorPlaceDown;
    public Transform doorPlaceLeft;
    public Transform doorPlaceRight;

    const string doorName = "Door";
    const string wallName = "Wall";

    private List<EnemyBasic> activeEnemies = new();
    private bool doorsReplaced = false;
    private bool hasSpawnedEnemies = false;

    private RoomEnemySpawner spawner;

    private void Awake()
    {
        if (doorPlaceUp == null) doorPlaceUp = transform.Find("DoorPlace_Up");
        if (doorPlaceDown == null) doorPlaceDown = transform.Find("DoorPlace_Down");
        if (doorPlaceLeft == null) doorPlaceLeft = transform.Find("DoorPlace_Left");
        if (doorPlaceRight == null) doorPlaceRight = transform.Find("DoorPlace_Right");

        spawner = GetComponentInChildren<RoomEnemySpawner>();
    }

    private void Start()
    {
        CacheDoorTriggers();
    }

    // Called by MapGenerator
    public void SetDoorState(bool upOpen, bool downOpen, bool leftOpen, bool rightOpen)
    {
        SetChildActive(doorPlaceUp, doorName, upOpen);
        SetChildActive(doorPlaceUp, wallName, !upOpen);

        SetChildActive(doorPlaceDown, doorName, downOpen);
        SetChildActive(doorPlaceDown, wallName, !downOpen);

        SetChildActive(doorPlaceLeft, doorName, leftOpen);
        SetChildActive(doorPlaceLeft, wallName, !leftOpen);

        SetChildActive(doorPlaceRight, doorName, rightOpen);
        SetChildActive(doorPlaceRight, wallName, !rightOpen);
    }

    private void SetChildActive(Transform parent, string childName, bool active)
    {
        if (parent == null) return;
        var child = parent.Find(childName);
        if (child != null)
            child.gameObject.SetActive(active);
    }

    // ----- DOOR TRIGGERS -----
    private List<DoorTrigger> cachedDoorTriggers = new();

    private void CacheDoorTriggers()
    {
        cachedDoorTriggers.Clear();
        TryAddTriggerFromPlace(doorPlaceUp);
        TryAddTriggerFromPlace(doorPlaceDown);
        TryAddTriggerFromPlace(doorPlaceLeft);
        TryAddTriggerFromPlace(doorPlaceRight);
    }

    private void TryAddTriggerFromPlace(Transform place)
    {
        if (place == null) return;
        var door = place.Find(doorName);
        if (door == null) return;
        var dt = door.GetComponent<DoorTrigger>();
        if (dt != null && !cachedDoorTriggers.Contains(dt))
            cachedDoorTriggers.Add(dt);
    }

    public void SetDoorTriggersActive(bool active)
    {
        foreach (var dt in cachedDoorTriggers)
        {
            if (dt != null)
                dt.enabled = active;
        }
    }

    // ----- ENEMY MANAGEMENT -----
    public void RegisterEnemy(EnemyBasic enemy)
    {
        if (enemy == null) return;
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyBasic enemy)
    {
        if (enemy == null) return;
        activeEnemies.Remove(enemy);

        if (activeEnemies.Count == 0)
        {
            RestoreDoors(); // ⬅️ Modified: now only opens valid doors
            Debug.Log($"Room {gridPosition} cleared, doors reopened!");
        }
    }

    public void SpawnEnemiesIfNeeded()
    {
        if (roomType == RoomType.Regular && !hasSpawnedEnemies && spawner != null)
        {
            spawner.SpawnEnemies();
            hasSpawnedEnemies = true;
        }

        // Doors become walls when enemies spawn
        if (spawner != null)
            ReplaceDoorsWithWalls();

        SetRoomActive(true);
    }

    public void SetRoomActive(bool active)
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                enemy.SetAIActive(active);
        }
    }

    // ----- DOOR/WALL LOGIC -----
    private void ReplaceDoorsWithWalls()
    {
        if (doorsReplaced) return;
        doorsReplaced = true;

        ToggleAllDoors(false);
    }

    // ⬇️ MODIFIED VERSION
    private void RestoreDoors()
    {
        if (!doorsReplaced) return;
        doorsReplaced = false;

        // Only reopen connected doors
        OpenConnectedDoorsOnly();
    }

    // ⬇️ NEW METHOD — checks which sides have actual connected rooms
    private void OpenConnectedDoorsOnly()
    {
        var roomMgr = RoomManager.Instance;
        if (roomMgr == null)
        {
            Debug.LogWarning("[RoomController] No RoomManager found!");
            return;
        }

        // Determine which directions have valid neighboring rooms
        bool up = roomMgr.createdRooms.Any(r => r.gridPosition == gridPosition + Vector2Int.up);
        bool down = roomMgr.createdRooms.Any(r => r.gridPosition == gridPosition + Vector2Int.down);
        bool left = roomMgr.createdRooms.Any(r => r.gridPosition == gridPosition + Vector2Int.left);
        bool right = roomMgr.createdRooms.Any(r => r.gridPosition == gridPosition + Vector2Int.right);

        // Open only the doors that lead to valid neighbor rooms
        SetChildActive(doorPlaceUp, doorName, up);
        SetChildActive(doorPlaceUp, wallName, !up);

        SetChildActive(doorPlaceDown, doorName, down);
        SetChildActive(doorPlaceDown, wallName, !down);

        SetChildActive(doorPlaceLeft, doorName, left);
        SetChildActive(doorPlaceLeft, wallName, !left);

        SetChildActive(doorPlaceRight, doorName, right);
        SetChildActive(doorPlaceRight, wallName, !right);
    }

    private void ToggleAllDoors(bool doorsActive)
    {
        SetChildActive(doorPlaceUp, doorName, doorsActive);
        SetChildActive(doorPlaceUp, wallName, !doorsActive);

        SetChildActive(doorPlaceDown, doorName, doorsActive);
        SetChildActive(doorPlaceDown, wallName, !doorsActive);

        SetChildActive(doorPlaceLeft, doorName, doorsActive);
        SetChildActive(doorPlaceLeft, wallName, !doorsActive);

        SetChildActive(doorPlaceRight, doorName, doorsActive);
        SetChildActive(doorPlaceRight, wallName, !doorsActive);
    }

    // Called when player enters via DoorTrigger
    public void OnPlayerEnteredRoom()
    {
        // Spawn enemies first time you enter
        SpawnEnemiesIfNeeded();

        // Activate their AI
        SetRoomActive(true);

        // Replace doors with walls while enemies exist
        if (activeEnemies.Count > 0)
            ReplaceDoorsWithWalls();
    }

    public void OnPlayerExitedRoom()
    {
        // Disable AI for performance
        SetRoomActive(false);
    }
}
