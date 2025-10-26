using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public float smoothTime = 0.3f;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private bool isMoving = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (isMoving)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void MoveToRoom(Vector3 roomCenter)
    {
        targetPosition = new Vector3(roomCenter.x, roomCenter.y, transform.position.z);
        isMoving = true;
    }
}
