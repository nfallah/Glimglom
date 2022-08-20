using UnityEngine;

public class ChunkOld : MonoBehaviour
{
    // Note: this was the original chunk creation script, and while with no material it looked the exact same (and more optimized)
    // -it had issues with uvs, pretty much it stretched out the entire sand material across the chunk rather than per 1x1 plane in that chunk
    // Though looking back now, we probably could've modified some things to make this script also work if we adjusted each coordinate to
    // display the entire sand using the global equation we already determined in lines 55-60
    public static int chunkSize = 16;

    public static float amplitude = 3f, scale = 2f;

    private Mesh mesh;

    private MeshFilter meshFilter;

    private MeshRenderer meshRenderer;

    private int[] triangles;

    private Vector2[] uv;

    private Vector3[] vertices;

    private void Start()
    {
        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter.mesh = mesh;
        GenerateQuads();
        UpdateMesh();
    }

    private void GenerateQuads()
    {
        triangles = new int[chunkSize * chunkSize * 6];
        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; i++, x++)
            {
                float y = amplitude * Mathf.PerlinNoise(scale * ((float)x / chunkSize), scale * ((float)z / chunkSize));
                vertices[i] = new Vector3(x - 0.5f, y, z - 0.5f);
                uv[i] = new Vector2((float)z / chunkSize, (float)x / chunkSize);
            }
        }

        for (int i = 0, z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; i += 6, x++)
            {
                triangles[i] = (chunkSize + 1) * z + x;
                triangles[i + 1] = (chunkSize + 1) * (z + 1) + x;
                triangles[i + 2] = (chunkSize + 1) * z + x + 1;
                triangles[i + 3] = (chunkSize + 1) * z + x + 1;
                triangles[i + 4] = (chunkSize + 1) * (z + 1) + x;
                triangles[i + 5] = (chunkSize + 1) * (z + 1) + x + 1;
            }
        }
    }

    private void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
    }
}