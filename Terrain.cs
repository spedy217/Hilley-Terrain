using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    public GameObject cube; // The cube prefab
    public GameObject player; // The player object
    public float noiseScale = 0.1f;
    public float heightMultiplier = 7f;
    public float hillHeight = 1.0f;

    int mapSize = 20; // Size of each chunk
    int chunkLoadRadius = 3; // Number of chunks to load around the player
    int chunkUnloadRadius = 5; // Unload chunks beyond this radius

    private Dictionary<Vector2Int, GameObject> chunkObjects = new Dictionary<Vector2Int, GameObject>();
    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    void Start()
    {
        StartCoroutine(LoadChunksCoroutine());
    }

    void Update()
    {
        // Player's current chunk
        Vector2Int playerChunk = GetChunkCoordinates(player.transform.position);
        
        // Load and unload chunks around the player
        LoadChunks(playerChunk);
        UnloadChunks(playerChunk);
    }

    IEnumerator LoadChunksCoroutine()
    {
        while (true)
        {
            Vector2Int playerChunk = GetChunkCoordinates(player.transform.position);
            LoadChunks(playerChunk);
            yield return new WaitForSeconds(0.5f); // Small delay to prevent frame drops
        }
    }

    void LoadChunks(Vector2Int playerChunk)
    {
        for (int x = -chunkLoadRadius; x <= chunkLoadRadius; x++)
        {
            for (int y = -chunkLoadRadius; y <= chunkLoadRadius; y++)
            {
                Vector2Int chunkCoords = new Vector2Int(playerChunk.x + x, playerChunk.y + y);
                if (!generatedChunks.Contains(chunkCoords))
                {
                    GenerateChunk(chunkCoords.x, chunkCoords.y);
                }
            }
        }
    }

    void UnloadChunks(Vector2Int playerChunk)
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in chunkObjects)
        {
            float distance = Vector2Int.Distance(playerChunk, chunk.Key);
            if (distance > chunkUnloadRadius)
            {
                Destroy(chunk.Value);
                chunksToRemove.Add(chunk.Key);
                generatedChunks.Remove(chunk.Key);
            }
        }

        foreach (var chunkKey in chunksToRemove)
        {
            chunkObjects.Remove(chunkKey);
        }
    }

    void GenerateChunk(int chunkX, int chunkY)
    {
        generatedChunks.Add(new Vector2Int(chunkX, chunkY));
        GameObject chunkParent = new GameObject($"Chunk_{chunkX}_{chunkY}");

        for (int x = 0; x <= mapSize; x++)
        {
            for (int y = 0; y <= mapSize; y++)
            {
                float rawHeight = Mathf.PerlinNoise((chunkX * mapSize + x) * noiseScale, (chunkY * mapSize + y) * noiseScale) * heightMultiplier;
                int blockHeight = Mathf.RoundToInt(rawHeight * hillHeight);

                for (int h = 0; h <= blockHeight; h++)
                {
                    Vector3 blockPosition = new Vector3(chunkX * mapSize + x, h, chunkY * mapSize + y);
                    GameObject block = Instantiate(cube, blockPosition, Quaternion.identity, chunkParent.transform);
                }
            }
        }

        chunkObjects[new Vector2Int(chunkX, chunkY)] = chunkParent;
    }

    Vector2Int GetChunkCoordinates(Vector3 position)
    {
        int chunkX = Mathf.FloorToInt(position.x / mapSize);
        int chunkY = Mathf.FloorToInt(position.z / mapSize);
        return new Vector2Int(chunkX, chunkY);
    }
}
