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
    public float inkFriction = 2f;
    public float normalFriction = 5f;
    public float dashTurnRate = 4f;

    [Header("Chaos Visuals")]
    public float chaosSpinSpeed = 360f;
    public float chaosIntensity = 1.5f;
    public float chaosRecoverySpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 inputDir;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    private bool movementEnabled = true;  // ✅ Movement toggle
    private float chaosAngleOffset = 0f;
    private float chaosSpinDirection = 1f;
    private float dashFriction = 5f;

    public GameObject projectilePrefab;   // Assign your Projectile prefab here
    public Transform firePoint;           // The point where the bullet will appear
    public float fireRate = 0.25f;        // Time between shots

    private float nextFireTime = 0f;

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

        //bool onInk = PaintManager.Instance.IsInked(transform.position);
        //dashFriction = onInk ? inkFriction : normalFriction;

        _animator.SetBool("IsRunning", inputDir.x != 0 || inputDir.y != 0 || isDashing);

        // Start dash
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && Time.time > nextDashTime)
            StartCoroutine(Dash());

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FixedUpdate()
    {
        if (!movementEnabled)
            return;

        if (isDashing)
            HandleDashMovement();
        else
            HandleNormalMovement();


        //if (rb.linearVelocity.magnitude > 0.1f)
        //    PaintManager.Instance.RemoveInkAtPosition(transform.position);
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

        // Add drift control
        rb.linearVelocity += inputDir * (moveSpeed * dashDriftControl * Time.fixedDeltaTime);

        // Apply friction (slows correctly)
        float slowFactor = Mathf.Clamp01(dashFriction * Time.fixedDeltaTime);
        rb.linearVelocity *= (1f - slowFactor);

        // Add chaotic spin
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
        chaosAngleOffset = 0f;

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    // ----------------------
    // 🔹 Utility Functions 🔹
    // ----------------------

    /// <summary>
    /// Instantly stops all movement and rotation.
    /// </summary>
    public void StopMovementImmediate()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        _animator?.SetBool("isRunning", false);
    }

    /// <summary>
    /// Enables or disables player movement entirely (useful for cutscenes, dialogues, etc).
    /// </summary>
    public void EnableMovement(bool enable)
    {
        movementEnabled = enable;

        if (!enable)
        {
            StopMovementImmediate();
        }
    }

    private void UpdateRotation()
    {
        Vector2 vel = rb.linearVelocity;
        if (vel.sqrMagnitude < 0.01f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * chaosRecoverySpeed);
            return;
        }

        float targetAngle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90f;

        if (isDashing)
        {
            float randomShake = Mathf.Sin(Time.time * 10f + Random.value * 3f) * 10f * chaosIntensity;
            float totalChaosAngle = targetAngle + chaosAngleOffset * 0.3f + randomShake;
            transform.rotation = Quaternion.Euler(0f, 0f, totalChaosAngle);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, targetAngle), Time.deltaTime * chaosRecoverySpeed);
            chaosAngleOffset = Mathf.Lerp(chaosAngleOffset, 0f, Time.deltaTime * chaosRecoverySpeed);
        }
    }

    void Shoot()
    {
        // Convert mouse to world position
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Make sure it's on same plane

        // Direction from firePoint to mouse
        Vector2 dir = (mouseWorldPos - firePoint.position).normalized;

        // Calculate angle
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Spawn projectile and rotate it correctly
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle));
        proj.GetComponent<PaintProjectile>().Initialize(dir);

        // For debugging
        Debug.DrawLine(firePoint.position, mouseWorldPos, Color.red, 0.5f);
        Debug.Log($"Mouse: {mouseWorldPos}, FirePoint: {firePoint.position}, Dir: {dir}, Angle: {angle}");
    }
}
