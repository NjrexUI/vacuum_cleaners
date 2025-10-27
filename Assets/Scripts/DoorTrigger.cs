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

        RoomManager.Instance.NotifyPlayerEnteredRoom(targetRoom);

        yield return RoomManager.Instance.MoveCameraToRoom(targetRoom);

        RoomManager.Instance.IsCameraMoving = false;
        isTransitioning = false;
    }
}
