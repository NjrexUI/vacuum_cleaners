using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    public List<RoomController> createdRooms = new();
    public Camera mainCamera;

    public float cameraSmoothTime = 0.15f;
    private bool isCameraMoving = false;

    private Vector3 cameraVelocity = Vector3.zero;

    public RoomController CurrentRoom { get; private set; }
    private RoomController previousRoom;

    private void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void RegisterGeneratedRooms(List<RoomController> rooms)
    {
        createdRooms = rooms;
        CurrentRoom = createdRooms.OrderBy(r => r.graphDistance).FirstOrDefault();

        if (CurrentRoom != null && mainCamera != null)
            mainCamera.transform.position = new Vector3(CurrentRoom.transform.position.x, CurrentRoom.transform.position.y, mainCamera.transform.position.z);

        MapUI.Instance?.BuildMap(createdRooms);

        // Disable all, then enable only current room’s neighbors
        foreach (var r in createdRooms) r.SetDoorTriggersActive(false);
        EnableNeighborRoomTriggers(CurrentRoom);
    }

    public void NotifyPlayerEnteredRoom(RoomController rc)
    {
        if (rc == CurrentRoom) return;

        previousRoom = CurrentRoom;
        CurrentRoom = rc;

        // Disable triggers of the room player entered
        rc.SetDoorTriggersActive(false);

        // Re-enable the previous room’s triggers (so you can go back)
        if (previousRoom != null)
            previousRoom.SetDoorTriggersActive(true);
    }

    public void EnableNeighborRoomTriggers(RoomController room)
    {
        if (room == null) return;

        // Disable everything first
        foreach (var r in createdRooms)
            r.SetDoorTriggersActive(false);

        // Enable only the current room's door triggers
        room.SetDoorTriggersActive(true);
    }

    public IEnumerator MoveCameraToRoom(RoomController targetRoom)
    {
        if (isCameraMoving) yield break;
        if (targetRoom == null || mainCamera == null) yield break;

        isCameraMoving = true;

        // Disable *all* door triggers globally during transition
        foreach (var r in createdRooms)
            r.SetDoorTriggersActive(false);

        // Disable player movement completely
        var player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement move = null;
        Rigidbody2D rb = null;

        if (player != null)
        {
            move = player.GetComponent<PlayerMovement>();
            rb = player.GetComponent<Rigidbody2D>();

            if (move != null)
            {
                move.enabled = false;
                move.StopInstantly(); // ✅ stop immediately, removes leftover velocity
            }

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        Vector3 targetPos = new Vector3(targetRoom.transform.position.x, targetRoom.transform.position.y, mainCamera.transform.position.z);
        Vector3 velocity = Vector3.zero;
        float threshold = 0.02f;
        float timeout = 1.5f;
        float elapsed = 0f;

        // Smooth camera move
        while (Vector3.Distance(mainCamera.transform.position, targetPos) > threshold && elapsed < timeout)
        {
            mainCamera.transform.position = Vector3.SmoothDamp(
                mainCamera.transform.position,
                targetPos,
                ref velocity,
                0.25f
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        CurrentRoom = targetRoom;
        isCameraMoving = false;

        // ✅ Re-enable only the *current room’s* door triggers
        EnableNeighborRoomTriggers(CurrentRoom);

        // ✅ Re-enable player movement now
        if (move != null)
        {
            move.enabled = true;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        yield break;
    }

    public bool IsCameraMoving => isCameraMoving;
}
