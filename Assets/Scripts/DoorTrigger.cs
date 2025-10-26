using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DoorTrigger : MonoBehaviour
{
    public RoomController targetRoom;          // Set by RoomController when doors are spawned
    public Vector2 playerSpawnOffset;          // Direction where player should appear in target room
    private bool isTransitioning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning || targetRoom == null) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(TransitionToRoom(other.transform));
    }

    private IEnumerator TransitionToRoom(Transform player)
    {
        isTransitioning = true;

        // Tell RoomManager to move camera
        yield return RoomManager.Instance.MoveCameraToRoom(targetRoom);

        // Move player slightly inside target room
        player.position = targetRoom.transform.position + (Vector3)playerSpawnOffset;

        isTransitioning = false;
    }
}
