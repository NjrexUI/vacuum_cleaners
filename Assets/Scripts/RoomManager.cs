using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    public List<RoomController> createdRooms = new List<RoomController>();

    // Camera smoothing settings
    [Header("Camera")]
    public Camera mainCamera;
    public float cameraSmoothTime = 0.15f; // lower = snappier
    private bool isCameraMoving = false;

    Vector3 cameraVelocity = Vector3.zero;
    RoomController currentRoom;

    private void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void RegisterGeneratedRooms(List<RoomController> rooms)
    {
        createdRooms = rooms;
        // Optionally find start/current based on spawn coords (graph distance 0)
        currentRoom = createdRooms.OrderBy(r => r.graphDistance).FirstOrDefault();
        // place camera initially at currentRoom center instantly
        if (currentRoom != null && mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, mainCamera.transform.position.z);
        }
        // tell MapUI to build minimap (if any)
        MapUI.Instance?.BuildMap(createdRooms);
    }

    public void NotifyPlayerEnteredRoom(RoomController rc)
    {
        if (rc == currentRoom) return;
        currentRoom = rc;
        // update camera target (smoothly in LateUpdate)
    }

    public IEnumerator MoveCameraToRoom(RoomController targetRoom)
    {
        Camera cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam == null || targetRoom == null) yield break;

        // set logical current room immediately (so other systems know)
        currentRoom = targetRoom;

        Vector3 start = cam.transform.position;
        Vector3 end = new Vector3(targetRoom.transform.position.x, targetRoom.transform.position.y, start.z);

        float elapsed = 0f;
        float duration = 0.35f; // tweak as desired

        isCameraMoving = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        cam.transform.position = end;
        isCameraMoving = false;
    }

    private void LateUpdate()
    {
        // If coroutine is actively moving camera, don't run SmoothDamp
        if (isCameraMoving) return;

        if (currentRoom == null || mainCamera == null) return;
        Vector3 target = new Vector3(currentRoom.transform.position.x, currentRoom.transform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, target, ref cameraVelocity, cameraSmoothTime);
    }
}
