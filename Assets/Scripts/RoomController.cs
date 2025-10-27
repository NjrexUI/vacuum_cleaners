using UnityEngine;
using System.Collections.Generic;
public class RoomController : MonoBehaviour
{
    public Vector2Int gridPosition;        
    public RoomType roomType = RoomType.Regular;
    public int graphDistance = 0;          

    private List<DoorTrigger> cachedDoorTriggers = new List<DoorTrigger>();

    public Transform doorPlaceUp;
    public Transform doorPlaceDown;
    public Transform doorPlaceLeft;
    public Transform doorPlaceRight;

    const string doorName = "Door";
    const string wallName = "Wall";

    private List<EnemyBasic> activeEnemies = new List<EnemyBasic>();
    private bool doorsReplaced = false;

    private void Start()
    {
        CacheDoorTriggers();
    }

    private void Awake()
    {
        if (doorPlaceUp == null) doorPlaceUp = transform.Find("DoorPlace_Up");
        if (doorPlaceDown == null) doorPlaceDown = transform.Find("DoorPlace_Down");
        if (doorPlaceLeft == null) doorPlaceLeft = transform.Find("DoorPlace_Left");
        if (doorPlaceRight == null) doorPlaceRight = transform.Find("DoorPlace_Right");
    }

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

    void SetChildActive(Transform parent, string childName, bool active)
    {
        if (parent == null) return;
        var child = parent.Find(childName);
        if (child != null)
            child.gameObject.SetActive(active);
    }

    public void CacheDoorTriggers()
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
        var door = place.Find("Door");
        if (door == null) return;
        var dt = door.GetComponent<DoorTrigger>();
        if (dt != null && !cachedDoorTriggers.Contains(dt))
            cachedDoorTriggers.Add(dt);
    }

    public void SetDoorTriggersActive(bool active)
    {
        for (int i = 0; i < cachedDoorTriggers.Count; i++)
        {
            var dt = cachedDoorTriggers[i];
            if (dt != null)
                dt.enabled = active;
        }
    }

    // --- NEW ---
    public void RegisterEnemy(EnemyBasic enemy)
    {
        if (enemy == null) return;
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            ReplaceDoorsWithWalls();
        }
    }

    public void UnregisterEnemy(EnemyBasic enemy)
    {
        if (enemy == null) return;
        activeEnemies.Remove(enemy);

        if (activeEnemies.Count == 0)
        {
            RestoreDoors();
        }
    }

    private void ReplaceDoorsWithWalls()
    {
        if (doorsReplaced) return;
        doorsReplaced = true;

        SetChildActive(doorPlaceUp, doorName, false);
        SetChildActive(doorPlaceUp, wallName, true);

        SetChildActive(doorPlaceDown, doorName, false);
        SetChildActive(doorPlaceDown, wallName, true);

        SetChildActive(doorPlaceLeft, doorName, false);
        SetChildActive(doorPlaceLeft, wallName, true);

        SetChildActive(doorPlaceRight, doorName, false);
        SetChildActive(doorPlaceRight, wallName, true);
    }

    private void RestoreDoors()
    {
        if (!doorsReplaced) return;
        doorsReplaced = false;

        SetChildActive(doorPlaceUp, doorName, true);
        SetChildActive(doorPlaceUp, wallName, false);

        SetChildActive(doorPlaceDown, doorName, true);
        SetChildActive(doorPlaceDown, wallName, false);

        SetChildActive(doorPlaceLeft, doorName, true);
        SetChildActive(doorPlaceLeft, wallName, false);

        SetChildActive(doorPlaceRight, doorName, true);
        SetChildActive(doorPlaceRight, wallName, false);
    }
    // ------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.NotifyPlayerEnteredRoom(this);
        }
    }
}