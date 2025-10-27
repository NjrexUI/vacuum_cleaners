/*using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int enemiesToSpawn = 2;

    [Header("Spawn Positions (relative to room center)")]
    public Vector2[] cornerOffsets = new Vector2[]
    {
        new Vector2(-4f, 4f),
        new Vector2(4f, 4f),
        new Vector2(-4f, -4f),
        new Vector2(4f, -4f)
    };

    private bool hasSpawned = false;
    private RoomController roomController;

    private void Awake()
    {
        roomController = GetComponentInParent<RoomController>();
    }

    public void SpawnEnemies()
    {
        if (hasSpawned || enemyPrefab == null || roomController == null)
            return;

        hasSpawned = true;

        int count = Mathf.Min(enemiesToSpawn, cornerOffsets.Length);

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + cornerOffsets[i];

            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, roomController.transform);

            EnemyBasic enemy = enemyObj.GetComponent<EnemyBasic>();
            if (enemy != null)
                roomController.RegisterEnemy(enemy);
        }
    }
}
*/

using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    [Range(0, 8)]
    public int enemiesToSpawn = 2;

    [Header("Spawn Positions (relative to room center)")]
    public Vector2[] cornerOffsets = new Vector2[]
    {
        new Vector2(-4f, 4f),
        new Vector2(4f, 4f),
        new Vector2(-4f, -4f),
        new Vector2(4f, -4f)
    };

    private bool hasSpawned = false;
    private RoomController roomController;

    private void Awake()
    {
        roomController = GetComponentInParent<RoomController>();
        if (roomController == null)
            Debug.LogWarning($"[RoomEnemySpawner] No RoomController found in parents of {name}");
    }

    // Public so RoomManager or RoomController can call it when appropriate
    public void SpawnEnemies()
    {
        if (hasSpawned)
        {
            Debug.Log($"[RoomEnemySpawner] already spawned in {name}");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError($"[RoomEnemySpawner] enemyPrefab not assigned on {name}");
            return;
        }

        if (roomController == null)
        {
            Debug.LogError($"[RoomEnemySpawner] cannot spawn because roomController is null on {name}");
            return;
        }

        hasSpawned = true;
        int count = Mathf.Min(enemiesToSpawn, cornerOffsets.Length);

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = (Vector2)transform.position + cornerOffsets[i];

            // instantiate as a child of the room so GetComponentInParent works reliably
            GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, roomController.transform);

            // try to set up enemy/room relationship immediately
            var enemy = go.GetComponent<EnemyBasic>();
            if (enemy != null)
            {
                // ensure enemy knows its room (method added below)
                enemy.AssignToRoom(roomController);
                // also register on the room side (safe even if enemy registers itself later)
                roomController.RegisterEnemy(enemy);
                Debug.Log($"[RoomEnemySpawner] Spawned and registered {enemy.name} in room {roomController.name}");
            }
            else
            {   
                Debug.LogWarning($"[RoomEnemySpawner] Spawned object has no EnemyBasic component: {go.name}");
            }
        }
    }

    // Optional helper for quick testing in editor: call from inspector context menu
    [ContextMenu("SpawnNow")]
    private void EditorSpawnNow()
    {
        SpawnEnemies();
    }
}
