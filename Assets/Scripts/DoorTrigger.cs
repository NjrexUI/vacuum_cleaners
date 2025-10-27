using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    [Tooltip("The room this door leads to.")]
    public RoomController targetRoom;

    [Tooltip("How far the player appears inside the next room.")]
    public float enterOffset = 0f;

    private bool isTransitioning = false;
    private Collider2D doorCollider;

    [HideInInspector] public RoomController parentRoom;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || isTransitioning || targetRoom == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (RoomManager.Instance.IsCameraMoving)
            return;

        if (RoomManager.Instance.CurrentRoom == targetRoom)
            return;

        isTransitioning = true;
        StartCoroutine(TransitionRoutine(other));
    }

    private IEnumerator TransitionRoutine(Collider2D player)
    {
        RoomManager.Instance.IsCameraMoving = true;

        var previousRoom = RoomManager.Instance.CurrentRoom;
        var newRoom = targetRoom;

        if (previousRoom != null)
            previousRoom.OnPlayerExitedRoom();

        // Notify manager
        RoomManager.Instance.NotifyPlayerEnteredRoom(newRoom);

        // Move camera
        yield return RoomManager.Instance.MoveCameraToRoom(newRoom);

        // Now that camera finished, activate enemies & seal doors
        newRoom.OnPlayerEnteredRoom();

        Vector3 enterDirection = (targetRoom.transform.position - player.transform.position).normalized;
        player.transform.position += enterDirection * 1.0f; // move 1 unit inside

        RoomManager.Instance.IsCameraMoving = false;
        isTransitioning = false;
    }
}
