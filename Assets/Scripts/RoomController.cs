using UnityEngine;
public class RoomController : MonoBehaviour
{
    public Vector2Int gridPosition;        // set by generator (x,y in grid)
    public RoomType roomType = RoomType.Regular;
    public int graphDistance = 0;          // distance from spawn (filled by generator)

    // Door placeholder transforms (assign to children or via code by name)
    public Transform doorPlaceUp;
    public Transform doorPlaceDown;
    public Transform doorPlaceLeft;
    public Transform doorPlaceRight;

    // Optionally references to actual door/wall objects to toggle:
    // If you put Door child GameObjects under placeholders, we just toggle them by name.
    const string doorName = "Door";
    const string wallName = "Wall";

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

    // Room enter/exit detection — requires the prefab root Collider2D set to isTrigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.NotifyPlayerEnteredRoom(this);
        }
    }
    public Transform GetDoorByName(string name)
    {
        return transform.Find($"Doors/{name}Door");
    }

}
