using UnityEngine;

public class Chunk : MonoBehaviour
{
    // Main chunk generation script that is in use
    public static int chunkSize = 16; // Size of each chunk in units
    public static float steepScale = 10f, steepAmplitude = 20f, amplitude = 3f, scale = 2f, yOffset = -100f; // Related perlin constants
    [SerializeField] Material sand; // Ground sand material

    public GameObject CreateChunk(Vector2Int chunkPos) // Creates a single chunk at a given chunk position (different from player position)
    {
        Vector2 offset = new Vector2(chunkPos.x * chunkSize, chunkPos.y * chunkSize);

        int chunkIndex = 0;

        GameObject chunk;

        Mesh chunkMesh;

        MeshFilter chunkMeshFilter;

        MeshRenderer chunkMeshRenderer;

        CombineInstance[] combineInstance;

        combineInstance = new CombineInstance[chunkSize * chunkSize];
        chunk = new GameObject("Chunk(" + chunkPos.x + ", " + chunkPos.y + ")");

        for (int z = 0; z < chunkSize; z++) // Here a bunch of quads (singular 1x1 planes) are created
        {
            for (int x = 0; x < chunkSize; x++)
            {
                CreateQuad(new Vector3(x + offset.x, z + offset.y), chunkIndex, combineInstance, chunk);
                chunkIndex++;
            }
        }

        // After creating each individual quad, they must be combined together for optimization and treated as a single mesh
        chunkMesh = new Mesh(); 
        chunkMeshFilter = chunk.AddComponent<MeshFilter>();
        chunkMeshFilter.mesh.Clear();
        chunkMeshFilter.mesh = chunkMesh;
        chunkMeshRenderer = chunk.AddComponent<MeshRenderer>();
        chunkMesh.CombineMeshes(combineInstance);
        chunkMeshRenderer.material = sand;
        chunk.AddComponent<MeshCollider>();
        chunkMeshFilter.mesh.RecalculateNormals();
        chunkMeshFilter.mesh.Optimize();

        foreach (Transform t in chunk.transform) // Destroys all previous quads as they were a reference and thus no longer needed
        {
            Destroy(t.gameObject);
        }

        return chunk;
    }

    // In short, makes a singular 1x1 plane mesh by adjusting the vertices, triangles, and uv values
    private void CreateQuad(Vector2 pos, int index, CombineInstance[] combine, GameObject chunkObject)
    {
        GameObject quad = new GameObject("Quad", typeof(MeshFilter), typeof(MeshRenderer));
        quad.transform.SetParent(chunkObject.transform);
        Mesh mesh = new Mesh();
        MeshFilter mf = quad.GetComponent<MeshFilter>();
        MeshRenderer mr = quad.GetComponent<MeshRenderer>();
        mf.mesh = mesh;
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(pos.x - 0.5f, yOffset + (amplitude * Mathf.PerlinNoise((float)(pos.x - 0.5f) / chunkSize * scale, (float)(pos.y - 0.5f) / chunkSize * scale)) /*+ (steepAmplitude * Mathf.PerlinNoise((float)(pos.x - 0.5f) / chunkSize * steepScale, (float)(pos.y - 0.5f) / chunkSize * steepScale))*/, pos.y - 0.5f),
            new Vector3(pos.x - 0.5f, yOffset + (amplitude * Mathf.PerlinNoise((float)(pos.x - 0.5f) / chunkSize * scale, (float)(pos.y + 0.5f) / chunkSize * scale)) /*+ (steepAmplitude * Mathf.PerlinNoise((float)(pos.x - 0.5f) / chunkSize * steepScale, (float)(pos.y + 0.5f) / chunkSize * steepScale))*/, pos.y + 0.5f),
            new Vector3(pos.x + 0.5f, yOffset + (amplitude * Mathf.PerlinNoise((float)(pos.x + 0.5f) / chunkSize * scale, (float)(pos.y + 0.5f) / chunkSize * scale)) /*+ (steepAmplitude * Mathf.PerlinNoise((float)(pos.x + 0.5f) / chunkSize * steepScale, (float)(pos.y + 0.5f) / chunkSize * steepScale))*/, pos.y + 0.5f),
            new Vector3(pos.x + 0.5f, yOffset + (amplitude * Mathf.PerlinNoise((float)(pos.x + 0.5f) / chunkSize * scale, (float)(pos.y - 0.5f) / chunkSize * scale)) /*+ (steepAmplitude * Mathf.PerlinNoise((float)(pos.x + 0.5f) / chunkSize * steepScale, (float)(pos.y - 0.5f) / chunkSize * steepScale))*/, pos.y - 0.5f)
        };
        int[] triangles = new int[] { 0, 1, 3, 3, 1, 2 };
        Vector2[] uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        combine[index].mesh = mf.sharedMesh;
        combine[index].transform = mf.transform.localToWorldMatrix;
    }

    // Static method used for the string, GameObject dictionary, allowing for infinite generation
    public static string Vector2IntToChunkPos(Vector2Int xzPos)
    {
        return "Chunk(" + xzPos.x + ", " + xzPos.y + ")";
    }
}