using UnityEngine;

public class EnemyBasic : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    private Transform player;
    private RoomController currentRoom;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float desiredDistance = 5f;

    [Header("Hover Motion")]
    public float hoverRadius = 0.2f;
    public float hoverSpeed = 2f;

    [Header("Shooting")]
    public float fireRate = 1.5f;
    private float nextFireTime;

    [Header("Health")]
    public int health = 3;

    private Rigidbody2D rb;
    private Vector2 hoverOffset;

    private bool aiActive = true;

    public void AssignToRoom(RoomController room)
    {
        currentRoom = room;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        RoomController room = GetComponentInParent<RoomController>();
        if (room != null)
            room.RegisterEnemy(this);

        SetAIActive(true);
    }

    public void SetAIActive(bool active)
    {
        aiActive = active;
        if (!aiActive && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (!aiActive || player == null) return;

        HandleMovement();
        HandleHover();
        HandleAiming();
        HandleShooting();
    }

    void HandleMovement()
    {
        Vector2 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > desiredDistance + 0.3f)
        {
            rb.MovePosition(rb.position + dir.normalized * moveSpeed * Time.deltaTime);
        }
        else if (dist < desiredDistance - 0.3f)
        {
            rb.MovePosition(rb.position - dir.normalized * moveSpeed * Time.deltaTime);
        }
    }

    void HandleHover()
    {
        hoverOffset.x = Mathf.Cos(Time.time * hoverSpeed) * hoverRadius;
        hoverOffset.y = Mathf.Sin(Time.time * hoverSpeed) * hoverRadius;
        transform.localPosition += (Vector3)(hoverOffset * Time.deltaTime);
    }

    void HandleAiming()
    {
        if (firePoint == null || player == null) return;

        Vector2 dir = (Vector2)(player.position - firePoint.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    void HandleShooting()
    {
        if (Time.time < nextFireTime) return;
        Shoot();
        nextFireTime = Time.time + fireRate;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        Instantiate(bulletPrefab, firePoint.position, rot);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        if (currentRoom != null)
            currentRoom.UnregisterEnemy(this);

        Destroy(gameObject);
    }
}
