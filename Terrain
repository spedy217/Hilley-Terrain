using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    public GameObject cube; // The cube prefab
    public GameObject player; // The player object
    float noiseScale = 0.1f; // Controls the scale of the Perlin noise
    float heightMultiplier = 7f; // Controls the height of the terrain
    int mapSize = 20; // Size of each chunk
    float chunkLoadDistance = 0.05f; // Distance to trigger new chunk generation

    private HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();

    void Start()
    {
        GenerateChunk(0, 0); // Generate the initial chunk
    }

    void Update()
    {
        // Get the player's current chunk coordinates
        Vector2Int playerChunk = GetChunkCoordinates(player.transform.position);

        // Check and generate surrounding chunks
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int chunkCoords = new Vector2Int(playerChunk.x + x, playerChunk.y + y);
                if (!generatedChunks.Contains(chunkCoords))
                {
                    GenerateChunk(chunkCoords.x, chunkCoords.y);
                }
            }
        }
    }

    void GenerateChunk(int chunkX, int chunkY)
    {
        generatedChunks.Add(new Vector2Int(chunkX, chunkY));
        Vector3 chunkOrigin = new Vector3(chunkX * mapSize, 0, chunkY * mapSize);

        for (int x = 0; x <= mapSize; x++)
        {
            for (int y = 0; y <= mapSize; y++)
            {
                float height = Mathf.PerlinNoise((chunkOrigin.x + x) * noiseScale, (chunkOrigin.z + y) * noiseScale) * heightMultiplier;
                Instantiate(cube, new Vector3(chunkOrigin.x + x, height, chunkOrigin.z + y), Quaternion.identity);
            }
        }
    }

    Vector2Int GetChunkCoordinates(Vector3 position)
    {
        int chunkX = Mathf.FloorToInt(position.x / mapSize);
        int chunkY = Mathf.FloorToInt(position.z / mapSize);
        return new Vector2Int(chunkX, chunkY);
    }
}
