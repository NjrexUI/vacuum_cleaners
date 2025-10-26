using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCarDashMovementChaotic : MonoBehaviour
{
    [Header("Normal Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 10f;
    public float friction = 12f;

    [Header("Dash Settings")]
    public float dashForce = 25f;
    public float dashDuration = 0.6f;
    public float dashCooldown = 1.2f;
    [Range(0f, 1f)] public float dashDriftControl = 0.5f;
    public float dashFriction = 2f;
    public float dashTurnRate = 4f;

    [Header("Chaos Visuals")]
    public float chaosSpinSpeed = 360f;        // Degrees per second of chaos spin
    public float chaosIntensity = 1.5f;        // How wild the rotation gets
    public float chaosRecoverySpeed = 6f;      // How quickly rotation returns to normal after dash

    private Rigidbody2D rb;
    private Vector2 inputDir;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    // Internal rotation chaos state
    private float chaosAngleOffset = 0f;
    private float chaosSpinDirection = 1f;

    [SerializeField] private Animator _animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    void Update()
    {
        // WASD input
        inputDir.x = Input.GetAxisRaw("Horizontal");
        inputDir.y = Input.GetAxisRaw("Vertical");
        inputDir.Normalize();

        
        _animator.SetBool("isRunning", inputDir.x != 0 || inputDir.y != 0);


        // Start dash
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && Time.time > nextDashTime)
            StartCoroutine(Dash());

        //UpdateRotation();
    }

    void FixedUpdate()
    {
        if (isDashing)
            HandleDashMovement();
        else
            HandleNormalMovement();
    }

    private void HandleNormalMovement()
    {
        Vector2 targetVelocity = inputDir * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * acceleration);

        if (inputDir == Vector2.zero)
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * friction);
    }

    private void HandleDashMovement()
    {
        // Curve dash direction slightly toward input
        if (inputDir.sqrMagnitude > 0.1f)
        {
            Vector2 currentDir = rb.linearVelocity.normalized;
            Vector2 desiredDir = Vector2.Lerp(currentDir, inputDir, Time.fixedDeltaTime * dashTurnRate).normalized;
            float currentSpeed = rb.linearVelocity.magnitude;
            rb.linearVelocity = desiredDir * currentSpeed;
        }

        // Add extra control acceleration
        rb.linearVelocity += inputDir * (moveSpeed * dashDriftControl * Time.fixedDeltaTime);

        // Apply friction
        float slowFactor = Mathf.Clamp01(dashFriction * Time.fixedDeltaTime);
        rb.linearVelocity *= (1f - slowFactor);

        // Add chaotic rotation torque
        chaosSpinDirection = Mathf.Sign(inputDir.x + Random.Range(-0.5f, 0.5f));
        chaosAngleOffset += chaosSpinDirection * chaosSpinSpeed * Time.deltaTime * Random.Range(0.7f, 1.3f);
    }

    private System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        Vector2 dashDir = inputDir.sqrMagnitude > 0.1f ? inputDir : rb.linearVelocity.normalized;
        if (dashDir == Vector2.zero)
            dashDir = transform.up;

        rb.linearVelocity = dashDir * dashForce;

        chaosAngleOffset = 0f; // Reset chaos at start of dash

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private void UpdateRotation()
    {
        Vector2 vel = rb.linearVelocity;
        if (vel.sqrMagnitude < 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * chaosRecoverySpeed);
            return;
        }

        // Base facing direction
        float targetAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90f;

        // Add chaos spin when dashing
        if (isDashing)
        {
            float randomShake = Mathf.Sin(Time.time * 10f + Random.value * 3f) * 10f * chaosIntensity;
            float totalChaosAngle = targetAngle + chaosAngleOffset * 0.3f + randomShake;
            transform.rotation = Quaternion.Euler(0f, 0f, totalChaosAngle);
        }
        else
        {
            // Smoothly reorient after dash
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, targetAngle), Time.deltaTime * chaosRecoverySpeed);
            chaosAngleOffset = Mathf.Lerp(chaosAngleOffset, 0f, Time.deltaTime * chaosRecoverySpeed);
        }
    }
}
