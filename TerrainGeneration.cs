using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    public Material terrainMaterial; // Assign your blocky terrain material in the Inspector
    int mapSize = 20;
    float noiseScale = 0.1f;
    float heightMultiplier = 5f;
    float chunkLoadDistance = 50f;

    private Dictionary<Vector2Int, GameObject> generatedChunks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        GenerateChunk(0, 0);
    }

    void Update()
    {
        Vector2Int playerChunk = GetChunkCoordinates(Camera.main.transform.position);

        // Generate surrounding chunks
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int chunkCoords = new Vector2Int(playerChunk.x + x, playerChunk.y + z);
                if (!generatedChunks.ContainsKey(chunkCoords))
                {
                    GenerateChunk(chunkCoords.x, chunkCoords.y);
                }
            }
        }

        UnloadDistantChunks(playerChunk);
    }

    void GenerateChunk(int chunkX, int chunkY)
    {
        Vector2Int chunkCoords = new Vector2Int(chunkX, chunkY);
        GameObject chunkObj = new GameObject($"Chunk_{chunkX}_{chunkY}");
        chunkObj.transform.position = new Vector3(chunkX * mapSize, 0, chunkY * mapSize);

        MeshFilter meshFilter = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;

        Mesh mesh = GenerateBlockyMesh(chunkX, chunkY);
        meshFilter.mesh = mesh;

        MeshCollider meshCollider = chunkObj.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        generatedChunks.Add(chunkCoords, chunkObj);
    }

    Mesh GenerateBlockyMesh(int chunkX, int chunkY)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        int vertIndex = 0;

        for (int x = 0; x < mapSize; x++)
        {
            for (int z = 0; z < mapSize; z++)
            {
                int worldX = chunkX * mapSize + x;
                int worldZ = chunkY * mapSize + z;

                float height = Mathf.Floor(Mathf.PerlinNoise(worldX * noiseScale, worldZ * noiseScale) * heightMultiplier);

                // Create a cube at each (x, height, z) position
                AddBlock(vertices, triangles, uvs, new Vector3(x, height, z), ref vertIndex);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }

    void AddBlock(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 position, ref int vertIndex)
    {
        // Cube vertices and triangles for each face
        Vector3[] faceVertices = new Vector3[]
        {
            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0), // Front
            new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), // Right
            new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 1), // Back
            new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1), // Left
            new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1), // Top
            new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 0, 0)  // Bottom
        };

        int[] faceTriangles = new int[]
        {
            0, 2, 1, 0, 3, 2, 4, 6, 5, 4, 7, 6,
            8, 10, 9, 8, 11, 10, 12, 14, 13, 12, 15, 14,
            16, 18, 17, 16, 19, 18, 20, 22, 21, 20, 23, 22
        };

        // Add cube's vertices and triangles at the correct position
        foreach (Vector3 vert in faceVertices)
        {
            vertices.Add(position + vert);
        }

        for (int i = 0; i < faceTriangles.Length; i++)
        {
            triangles.Add(faceTriangles[i] + vertIndex);
        }

        vertIndex += faceVertices.Length;
    }

    void UnloadDistantChunks(Vector2Int playerChunk)
    {
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in generatedChunks)
        {
            float distance = Vector2.Distance(new Vector2(playerChunk.x, playerChunk.y), new Vector2(chunk.Key.x, chunk.Key.y));
            if (distance > chunkLoadDistance / mapSize)
            {
                Destroy(chunk.Value);
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (Vector2Int chunkCoord in chunksToRemove)
        {
            generatedChunks.Remove(chunkCoord);
        }
    }

    Vector2Int GetChunkCoordinates(Vector3 position)
    {
        int chunkX = Mathf.FloorToInt(position.x / mapSize);
        int chunkY = Mathf.FloorToInt(position.z / mapSize);
        return new Vector2Int(chunkX, chunkY);
    }
}
