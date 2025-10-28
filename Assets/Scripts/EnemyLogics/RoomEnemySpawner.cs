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
            Vector2 spawnPos = (Vector2)roomController.transform.position + cornerOffsets[i];
            GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, roomController.transform);

            var enemy = go.GetComponent<EnemyBasic>();
            if (enemy != null)
            {
                enemy.AssignToRoom(roomController);
                roomController.RegisterEnemy(enemy);
                Debug.Log($"[RoomEnemySpawner] Spawned {enemy.name} in {roomController.name}");
            }
            else
            {
                Debug.LogWarning($"[RoomEnemySpawner] Spawned object has no EnemyBasic: {go.name}");
            }
        }
    }

    [ContextMenu("SpawnNow")]
    private void EditorSpawnNow()
    {
        SpawnEnemies();
    }
}
