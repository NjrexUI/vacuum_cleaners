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
        if (!enabled || isTransitioning || targetRoom == null) return;
        if (!other.CompareTag("Player")) return;
        if (RoomManager.Instance.IsCameraMoving) return;
        if (RoomManager.Instance.CurrentRoom == targetRoom) return;

        StartCoroutine(TransitionToRoom(other.transform));
    }

    private IEnumerator TransitionToRoom(Transform player)
    {
        Vector3 doorPos = transform.position;
        Vector3 playerStart = player.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        PlayerMovement move = player.GetComponent<PlayerMovement>();

        Vector3 lastVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

        if (move != null)
        {
            move.StopInstantly();
            move.enabled = false;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return RoomManager.Instance.MoveCameraToRoom(targetRoom);
        yield return null;

        Vector3 direction = lastVelocity.sqrMagnitude > 0.001f
            ? lastVelocity.normalized
            : (doorPos - playerStart).normalized;

        Vector3 spawnPos = doorPos;

        int safety = 0;
        while (Physics2D.OverlapCircle(spawnPos, 0.15f, LayerMask.GetMask("Walls", "Doors")) && safety < 5)
        {
            spawnPos += direction * 0.3f;
            safety++;
        }

        player.position = new Vector3(spawnPos.x, spawnPos.y, player.position.z);

        yield return new WaitForSeconds(0.15f);
        if (move != null) move.enabled = true;

        isTransitioning = false;

    }
}
