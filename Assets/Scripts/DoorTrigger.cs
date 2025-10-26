using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public RoomController targetRoom;
    public Transform targetSpawnPoint;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;
        if (!other.CompareTag("Player")) return;
        if (targetRoom == null) return;

        StartCoroutine(Transition(other.gameObject));
    }

    private System.Collections.IEnumerator Transition(GameObject player)
    {
        isTransitioning = true;

        // Optional: fade or disable player input
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Move player instantly to target room's spawn point
        player.transform.position = targetSpawnPoint.position;

        // Move camera to target room
        CameraController.Instance.MoveToRoom(targetRoom.transform.position);

        yield return new WaitForSeconds(0.4f); // small delay
        isTransitioning = false;
    }
}
