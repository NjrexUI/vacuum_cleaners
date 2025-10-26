using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 12f;
    public float deceleration = 10f;

    [Header("Optional")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // Don�t allow input while camera transitions are happening
        if (RoomManager.Instance != null && RoomManager.Instance.IsCameraMoving)
        {
            moveInput = Vector2.zero;
            if (animator) animator.SetFloat("Speed", 0);
            return;
        }

        // Get movement input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        moveInput = moveInput.normalized;

        // Update animation parameters
        if (animator)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }

        // Flip sprite based on direction (optional)
        if (spriteRenderer && moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }

    void FixedUpdate()
    {
        // Smooth acceleration & deceleration
        Vector2 targetVelocity = moveInput * moveSpeed;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity,
            (moveInput.sqrMagnitude > 0 ? acceleration : deceleration) * Time.fixedDeltaTime);

        rb.linearVelocity = currentVelocity;
    }

    public void StopInstantly()
    {
        rb.linearVelocity = Vector2.zero;
        currentVelocity = Vector2.zero;
    }
}
