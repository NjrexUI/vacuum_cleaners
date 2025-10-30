using UnityEngine;

[RequireComponent(typeof(RoomController))]
public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    [Range(1, 8)] public int enemiesToSpawn = 3;

    [Header("Spawn Settings")]
    [Tooltip("How many random tries to find a valid spawn point inside the room collider.")]
    public int maxSpawnAttempts = 30;

    [Tooltip("Extra margin to keep enemies away from walls.")]
    public float wallMargin = 1f;

    [Tooltip("Minimum distance between spawned enemies.")]
    public float minDistanceBetweenEnemies = 5f;

    private bool hasSpawned = false;
    private RoomController roomController;
    private Collider2D roomCollider;

    private readonly System.Collections.Generic.List<Vector2> spawnedPositions = new();

    private void Awake()
    {
        roomController = GetComponentInParent<RoomController>();
        if (roomController == null)
        {
            Debug.LogError($"[RoomEnemySpawner] ❌ No RoomController found in parents of {name}");
            return;
        }

        roomCollider = roomController.GetComponentInChildren<Collider2D>();
        if (roomCollider == null)
        {
            Debug.LogError($"[RoomEnemySpawner] ❌ No Collider2D found in {roomController.name} or its children!");
        }
        else
        {
            Debug.Log($"[RoomEnemySpawner] ✅ Found collider: {roomCollider.name}");
        }
    }

    public void SpawnEnemies()
    {
        if (hasSpawned)
        {
            Debug.Log($"[RoomEnemySpawner] Already spawned enemies in {name}");
            return;
        }

        if (enemyPrefab == null || roomCollider == null)
        {
            Debug.LogError($"[RoomEnemySpawner] ❌ Missing prefab or collider!");
            return;
        }

        hasSpawned = true;

        Bounds bounds = roomCollider.bounds;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector2 spawnPos = GetValidSpawnPosition(bounds);

            GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, roomController.transform);

            var enemy = go.GetComponent<EnemyBasic>();
            if (enemy != null)
            {
                enemy.AssignToRoom(roomController);
                roomController.RegisterEnemy(enemy);
            }
            else
            {
                Debug.LogWarning($"[RoomEnemySpawner] Spawned object has no EnemyBasic: {go.name}");
            }
        }

        Debug.Log($"[RoomEnemySpawner] ✅ Spawned {enemiesToSpawn} enemies inside {roomController.name}");
    }

    private Vector2 GetValidSpawnPosition(Bounds bounds)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float x = Random.Range(bounds.min.x + wallMargin, bounds.max.x - wallMargin);
            float y = Random.Range(bounds.min.y + wallMargin, bounds.max.y - wallMargin);
            Vector2 candidate = new Vector2(x, y);

            // Check if inside room collider
            if (!roomCollider.OverlapPoint(candidate))
                continue;

            // Check distance from already spawned enemies
            bool tooClose = false;
            foreach (var pos in spawnedPositions)
            {
                if (Vector2.Distance(candidate, pos) < minDistanceBetweenEnemies)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            // Save and return if valid
            spawnedPositions.Add(candidate);
            return candidate;
        }

        // fallback to center if all attempts failed
        Debug.LogWarning($"[RoomEnemySpawner] Could not find valid spawn point after {maxSpawnAttempts} attempts. Using center.");
        return roomCollider.bounds.center;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (roomController == null) roomController = GetComponentInParent<RoomController>();
        if (roomController == null) return;

        Collider2D col = roomController.GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
#endif
}
