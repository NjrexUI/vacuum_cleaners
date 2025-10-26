using UnityEngine;
using System.Collections.Generic;
public class RoomController : MonoBehaviour
{
    public Vector2Int gridPosition;        // set by generator (x,y in grid)
    public RoomType roomType = RoomType.Regular;
    public int graphDistance = 0;          // distance from spawn (filled by generator)

    private List<DoorTrigger> cachedDoorTriggers = new List<DoorTrigger>();

    // Door placeholder transforms (assign to children or via code by name)
    public Transform doorPlaceUp;
    public Transform doorPlaceDown;
    public Transform doorPlaceLeft;
    public Transform doorPlaceRight;

    // Optionally references to actual door/wall objects to toggle:
    // If you put Door child GameObjects under placeholders, we just toggle them by name.
    const string doorName = "Door";
    const string wallName = "Wall";

    private void Start()
    {
        CacheDoorTriggers();
    }

    private void Awake()
    {
        // Try to auto-assign the placeholders if not set in inspector
        if (doorPlaceUp == null) doorPlaceUp = transform.Find("DoorPlace_Up");
        if (doorPlaceDown == null) doorPlaceDown = transform.Find("DoorPlace_Down");
        if (doorPlaceLeft == null) doorPlaceLeft = transform.Find("DoorPlace_Left");
        if (doorPlaceRight == null) doorPlaceRight = transform.Find("DoorPlace_Right");
    }

    // Called from generator after creating neighbors boolean
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

    // Enable/disable *this room's* door triggers
    public void SetDoorTriggersActive(bool active)
    {
        for (int i = 0; i < cachedDoorTriggers.Count; i++)
        {
            var dt = cachedDoorTriggers[i];
            if (dt != null)
                dt.enabled = active;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.NotifyPlayerEnteredRoom(this);
        }
    }
}
