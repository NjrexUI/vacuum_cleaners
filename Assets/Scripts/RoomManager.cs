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

        foreach (var r in createdRooms) r.SetDoorTriggersActive(false);
        EnableNeighborRoomTriggers(CurrentRoom);
    }

    public void NotifyPlayerEnteredRoom(RoomController rc)
    {
        if (rc == CurrentRoom) return;

        previousRoom = CurrentRoom;
        CurrentRoom = rc;

        if (CurrentRoom != null)
            CurrentRoom.SetDoorTriggersActive(false);

        EnableNeighborRoomTriggers(CurrentRoom);

        if (previousRoom != null && previousRoom != CurrentRoom)
        {
            previousRoom.SetDoorTriggersActive(false);
        }
    }

    public void EnableNeighborRoomTriggers(RoomController room)
    {
        if (room == null) return;

        Vector2Int pos = room.gridPosition;

        var dirUp = pos + Vector2Int.up;
        var dirDown = pos + Vector2Int.down;
        var dirLeft = pos + Vector2Int.left;
        var dirRight = pos + Vector2Int.right;

        void EnableIfExists(Vector2Int p)
        {
            var neighbor = createdRooms.FirstOrDefault(r => r.gridPosition == p);
            if (neighbor != null)
                neighbor.SetDoorTriggersActive(true);
        }

        EnableIfExists(dirUp);
        EnableIfExists(dirDown);
        EnableIfExists(dirLeft);
        EnableIfExists(dirRight);
    }

    public IEnumerator MoveCameraToRoom(RoomController targetRoom)
    {
        if (isCameraMoving) yield break;
        if (targetRoom == null || mainCamera == null) yield break;

        isCameraMoving = true;

        foreach (var r in createdRooms)
            r.SetDoorTriggersActive(false);

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
                move.StopInstantly();
            }

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        Vector3 targetPos = new Vector3(targetRoom.transform.position.x, targetRoom.transform.position.y, mainCamera.transform.position.z);
        Vector3 velocity = Vector3.zero;
        float threshold = 0.02f;
        float timeout = 1.5f;
        float elapsed = 0f;

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

        EnableNeighborRoomTriggers(CurrentRoom);

        if (move != null)
        {
            move.enabled = true;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        yield break;
    }
    public bool IsCameraMoving { get; set; }
}
