/*
public class PlayerShooting : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireRate = 0.2f;
    public float fireOffset = 0.5f; // distance from player center

    private Camera mainCam;
    private float nextFireTime;

    private void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
            Debug.LogError("[PlayerShooting] ❌ No Main Camera found in scene!");
    }

    private void Update()
    {
        if (Input.GetButton("Fire1")) // left mouse button
            TryShoot();
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        Shoot();
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[PlayerShooting] ❌ Projectile prefab not assigned!");
            return;
        }

        // calculate direction and spawn position
        Vector2 aimDir = GetAimDirection();
        Vector3 spawnPos = transform.position + (Vector3)(aimDir * fireOffset);

        // spawn projectile
        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = aimDir * projectileSpeed;

        // rotate projectile to face direction
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // optional: destroy after 5s
        Destroy(bullet, 5f);
    }

    private Vector2 GetAimDirection()
    {
        // Convert mouse to world coordinates (2D orthographic camera)
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f; // ensure same plane

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;

        if (dir.sqrMagnitude == 0f)
            dir = Vector2.up;

        // visualize direction
        Debug.DrawLine(transform.position, mouseWorld, Color.green, 0.1f);
        Debug.Log($"MouseWorld: {mouseWorld}, Player: {transform.position}");

        return dir;
    }
}
*/

using UnityEngine;
using System.Linq;

public class PlayerShooting : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireRate = 0.2f;
    public float fireOffset = 0.5f;

    [Header("Auto Aim Settings")]
    [Tooltip("Maximum distance to auto-lock onto enemies.")]
    public float autoAimRange = 8f;
    [Tooltip("Enemies must have this tag to be targeted.")]
    public string enemyTag = "Enemy";

    private Camera mainCam;
    private float nextFireTime;

    private void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
            Debug.LogError("[PlayerShooting] ❌ No Main Camera found in scene!");
    }

    private void Update()
    {
        if (Input.GetButton("Fire1")) // left mouse button
            TryShoot();
    }

    private void TryShoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        Shoot();
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[PlayerShooting] ❌ Projectile prefab not assigned!");
            return;
        }

        // Get final aim direction (auto-aim or fallback to mouse)
        Vector2 aimDir = GetAutoAimDirection();

        // Compute spawn position based on offset
        Vector3 spawnPos = transform.position + (Vector3)(aimDir * fireOffset);

        // Instantiate projectile
        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = aimDir * projectileSpeed;

        // Rotate projectile to face direction
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Cleanup
        Destroy(bullet, 5f);
    }

    private Vector2 GetAutoAimDirection()
    {
        Vector2 playerPos = transform.position;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        GameObject nearestEnemy = enemies
            .OrderBy(e => Vector2.Distance(playerPos, e.transform.position))
            .FirstOrDefault(e => Vector2.Distance(playerPos, e.transform.position) <= autoAimRange);

        Vector2 aimDir;

        if (nearestEnemy != null)
        {
            // ✅ Aim at nearest enemy
            Vector2 targetPos = nearestEnemy.transform.position;
            aimDir = (targetPos - playerPos).normalized;
            Debug.DrawLine(playerPos, targetPos, Color.red, 0.15f);
        }
        else
        {
            // 🧭 Fallback to mouse aim if no enemies nearby
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            aimDir = ((Vector2)mouseWorld - playerPos).normalized;
            if (aimDir.sqrMagnitude == 0) aimDir = Vector2.up;
            Debug.DrawLine(playerPos, mouseWorld, Color.green, 0.15f);
        }

        return aimDir;
    }
}
