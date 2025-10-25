using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private int[] floorPlan;

    private int floorPlanCount;
    private int minRooms;
    private int maxRooms;
    private List<int> endRooms;

    private int bossRoomIndex;
    private int secretRoomIndex;
    private int shopRoomIndex;

    public Cell cellPrefab;
    private float cellSize;
    private Queue<int> cellQueue;
    private List<Cell> spawnedCells;

    [Header("Sprite References")]
    [SerializeField] private Sprite shop;
    [SerializeField] private Sprite boss;
    [SerializeField] private Sprite secret;

    void Start() 
    {
        minRooms = 7;
        maxRooms = 15;
        cellSize = 0.5f;
        spawnedCells = new();

        SetupLocation();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetupLocation();
        }
    }

    void SetupLocation() 
    {
        for (int i = 0; i < spawnedCells.Count; i++)
            Destroy(spawnedCells[i].gameObject);

        spawnedCells.Clear();

        floorPlan = new int[100];
        floorPlanCount = default;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();

        VisitCell(45);

        GenerateLocation();
    }

    void GenerateLocation() 
    {
        while (cellQueue.Count > 0)
        {
            int index = cellQueue.Dequeue();
            int x = index % 10;

            bool created = false;

            if (x > 1) created |= VisitCell(index - 1);
            if (x < 9) created |= VisitCell(index + 1);
            if (index > 20) created |= VisitCell(index - 10);
            if (index < 70) created |= VisitCell(index + 10);

            if (created == false)
                endRooms.Add(index);
        }

        if (floorPlanCount < minRooms)
        {
            SetupLocation();
            return;
        }

        SetupSpecialRooms();
    }

    void SetupSpecialRooms() 
    {
        bossRoomIndex = endRooms.Count > 0 ? endRooms[endRooms.Count - 1] : -1;

        if (bossRoomIndex != -1)
            endRooms.RemoveAt(endRooms.Count - 1);

        shopRoomIndex = RandomEndRoom();
        secretRoomIndex = PickSecretRoom();

        if (bossRoomIndex == -1 || shopRoomIndex == -1 || secretRoomIndex == -1)
        {
            SetupLocation();
            return;
        }

        SpawnRoom(secretRoomIndex);
        UpdateSpecialRoomVisuals();
    }

    void UpdateSpecialRoomVisuals() 
    {
        foreach(var cell in spawnedCells)
        {
            if (cell.index == shopRoomIndex)
                cell.SetSpecialRoomSprite(shop);

            if (cell.index == bossRoomIndex)
                cell.SetSpecialRoomSprite(boss);

            if (cell.index == secretRoomIndex)
                cell.SetSpecialRoomSprite(secret);
        }
    }

    int RandomEndRoom() 
    {
        if (endRooms.Count == 0) return -1;

        int randomRoom = Random.Range(0, endRooms.Count);
        int index = endRooms[randomRoom];

        endRooms.RemoveAt(randomRoom);

        return index;
    }

    int PickSecretRoom()
    {
        for (int attempt = 0; attempt < 900; attempt++)
        {
            int x = Mathf.FloorToInt(Random.Range(0f, 1f) * 9) + 1;
            int y = Mathf.FloorToInt(Random.Range(0f, 1f) * 8) + 2;

            int index = y * 10 + x;

            if (floorPlan[index] != 0)
                continue;

            if (bossRoomIndex == index - 1 || bossRoomIndex == index + 1 || bossRoomIndex == index + 10 || bossRoomIndex == index - 10)
                continue;

            if (index - 1 < 0 || index + 1 > floorPlan.Length || index - 10 < 0 || index + 10 > floorPlan.Length)
                continue;

            int neighbours = GetNeighbourCount(index);
            if (neighbours >= 3 || (attempt > 300 && neighbours >= 2) || (attempt > 600 && neighbours >= 1))
                return index;
        }

        return -1;
    }

    private int GetNeighbourCount(int index)
    {
        return floorPlan[index - 10] + floorPlan[index - 1] + floorPlan[index + 1] + floorPlan[index + 10];
    }

    private bool VisitCell(int index) 
    {
        if (floorPlan[index] != 0 || GetNeighbourCount(index) > 1 || floorPlanCount > maxRooms || Random.value < 0.5f)
            return false;

        cellQueue.Enqueue(index);
        floorPlan[index] = 1;
        floorPlanCount++;

        SpawnRoom(index);

        return true;
    }

    private void SpawnRoom(int index)
    { 
        int gridWidth = 10;
        int gridHeight = 10;
        
        int x = index % gridWidth;
        int y = index / gridHeight;


        Vector2 offset = new Vector2((gridWidth - 1) * 0.5f, (gridHeight - 1) * 0.5f) * cellSize;
        Vector2 position = new Vector2(x * cellSize, -(y * cellSize)) + new Vector2(0, (gridHeight - 1) * 0.5f * cellSize) - new Vector2(offset.x, 0);

        Cell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        newCell.value = 1;
        newCell.index = index;

        spawnedCells.Add(newCell);
    }
}
