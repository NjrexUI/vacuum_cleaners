using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<Room> createdRooms;

    [Header("Offset Variables")]
    public float offsetX;
    public float offsetY;

    [Header("Prefab References")]
    public Room roomPrefab;
    public Door doorPrefab;

    [Header("Scriptable Object References")]
    public DoorScriptable[] doors;
    public RoomScriptable[] rooms;

    [Header("Tilemap Room Prefabs")]
    public GameObject regularRoomPrefab;
    public GameObject shopRoomPrefab;
    public GameObject bossRoomPrefab;
    public GameObject secretRoomPrefab;

    public static RoomManager instance;

    private void Awake()
    {
        instance = this;
        createdRooms = new List<Room>();
    }

    public void SetupRooms(List<Cell> spawnedCells)
    {
        for (int i = createdRooms.Count - 1; i >= 0; i--)
            Destroy(createdRooms[i].gameObject);
        createdRooms.Clear();

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var cell in spawnedCells)
        {
            Vector2 pos = cell.transform.position;
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }

        Vector2 gridCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

        foreach (var currentCell in spawnedCells)
        {
            var foundRoom = rooms.FirstOrDefault(x => x.roomShape == currentCell.roomShape && x.roomType == currentCell.roomType && DoesTileMatchCell(x.occupiedTiles, currentCell));

            var currentPosition = (Vector2)currentCell.transform.position - gridCenter;

            var convertedPosition = new Vector2(currentPosition.x * offsetX, currentPosition.y * offsetY);

            GameObject prefabToSpawn = null;

            switch (currentCell.roomType)
            {
                case RoomType.Shop:
                    prefabToSpawn = shopRoomPrefab;
                    break;
                case RoomType.Boss:
                    prefabToSpawn = bossRoomPrefab;
                    break;
                case RoomType.Secret:
                    prefabToSpawn = secretRoomPrefab;
                    break;
                default:
                    prefabToSpawn = regularRoomPrefab;
                    break;
            }

            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"No prefab assigned for {currentCell.roomType}");
                continue;
            }

            GameObject newRoom = Instantiate(prefabToSpawn, convertedPosition, Quaternion.identity);

            Room roomComponent = newRoom.GetComponent<Room>();
            if (roomComponent != null)
            {
                roomComponent.SetupRoom(currentCell, foundRoom);
                createdRooms.Add(roomComponent);
            }
        }

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found in scene (tag 'Player'). Can't center map.");
            return;
        }

        Vector2 avg = Vector2.zero;
        foreach (var room in createdRooms)
            avg += (Vector2)room.transform.position;

        avg /= createdRooms.Count;

        Vector2 offset = (Vector2)player.transform.position - avg;

        foreach (var room in createdRooms)
            room.transform.position += (Vector3)offset;

    }

    private bool DoesTileMatchCell(int[] occupiedTiles, Cell cell)
    {
        if (occupiedTiles.Length != cell.cellList.Count)
            return false;

        int minIndex = cell.cellList.Min();
        List<int> normalizedCell = new List<int>();

        foreach (int index in cell.cellList)
        {
            int dx = (index % 10) - (minIndex % 10);
            int dy = (index / 10) - (minIndex / 10);

            normalizedCell.Add(dy * 10 + dx);
        }

        normalizedCell.Sort();
        int[] sortedOccupied = (int[])occupiedTiles.Clone();
        Array.Sort(sortedOccupied);

        return normalizedCell.SequenceEqual(sortedOccupied);
    }
}