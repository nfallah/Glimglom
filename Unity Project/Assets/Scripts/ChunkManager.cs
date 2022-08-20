using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // While this procedular generation system does work, it will not for instances where you travel more than chunkSize in a single frame.
    // With a chunkSize of 16, if the fish has such a high movement speed that you can travel 32 units for example, one chunk will go missing.
    // With the above example, you technically should still have the chunk work as intended if you move slower and go back near the missing chunk.
    public static int renderDistance = 10; // Determines how many chunks ahead of the player should be rendered

    [SerializeField] Transform player;
    [SerializeField] Chunk chunk; // Chunk script that must be on the same GameObject in scene in order to create chunks
    Dictionary<string, GameObject> chunks = new Dictionary<string, GameObject>(); // Infinite generation dictionary
    private Vector2Int oldPos, newPos;
    private List<Vector2Int> oldCoords, newCoords;
    private Queue<Vector2Int> loadQueue = new Queue<Vector2Int>(); // What to load/create
    private Queue<Vector2Int> unloadQueue = new Queue<Vector2Int>(); // What to unload
    private float chunkTimer;
    private bool loadQueueActive;
    private bool unloadQueueActive;

    private void Start()
    {
        // Creates all of the starting chunks around the player
        oldPos = playerPosToChunk();
        oldCoords = new List<Vector2Int>();
        newCoords = new List<Vector2Int>();
        chunkTimer = 5f / renderDistance * 0.1f;

        for (int z = -renderDistance + oldPos.y; z <= renderDistance + oldPos.y; z++)
        {
            for (int x = -renderDistance + oldPos.x; x <= renderDistance + oldPos.x; x++)
            {
                Vector2Int pos = new Vector2Int(x, z);
                GameObject newChunk = chunk.CreateChunk(pos);
                oldCoords.Add(pos);
                chunks.Add(newChunk.name, newChunk);
            }
        }
    }

    private void Update()
    {
        newPos = playerPosToChunk();

        if (newPos != oldPos) // If the player has for sure moved to a new chunk position, then we will bother doing anything at all
        {
            ChunkCheck(); // Main script

            if (!loadQueueActive) // Since we are 100% going to have new chunks to load, we must turn on the queue method if not on already
            {
                LoadQueue();
                loadQueueActive = true;
            }

            if (!unloadQueueActive) // Since we are 100% going to have old chunks to unload, we must turn on the dequeue method if not on already
            {
                UnloadQueue();
                unloadQueueActive = true;
            }
        }
    }

    private void LoadQueue() // Either creates completely new chunks or loads previously loaded chunks that are in queue (in view of player)
    {
        Vector2Int currentQueue = loadQueue.First();

        if (chunks.TryGetValue(Chunk.Vector2IntToChunkPos(currentQueue), out GameObject value)) // Case when chunk was once loaded before
        {
            value.GetComponent<MeshRenderer>().enabled = true;
            value.GetComponent<MeshCollider>().enabled = true;
        }

        else // Case when no such instance of a chunk exists and thus must be created
        {
            GameObject newChunk = chunk.CreateChunk(currentQueue);
            chunks.Add(newChunk.name, newChunk);
        }

        loadQueue.Dequeue();

        if (loadQueue.Count == 0) // Turns off automatically if queue is empty
        {
            loadQueueActive = false;
        }

        else // If queue is not empty, begin working in chunkTimer seconds (recursion)
        {
            Invoke("LoadQueue", chunkTimer);
        }
    }

    private void UnloadQueue() // Since in order to unload a chunk there must be a chunk loaded, we always unload the chunk at a coordinate
    {
        Vector2Int currentQueue = unloadQueue.First();

        string key = Chunk.Vector2IntToChunkPos(currentQueue);
        chunks[key].GetComponent<MeshRenderer>().enabled = false;
        chunks[key].GetComponent<MeshCollider>().enabled = false;

        unloadQueue.Dequeue();

        if (unloadQueue.Count == 0) // Turns off automatically if queue is empty
        {
            unloadQueueActive = false;
        }

        else // If queue is not empty, begin working in chunkTimer seconds (recursion)
        {
            Invoke("UnloadQueue", chunkTimer);
        }
    }

    private void ChunkCheck()
    {
        newCoords.Clear();

        // Here we look for all of the chunks around the new position of the player using the given render distance
        for (int z = -renderDistance + newPos.y; z <= renderDistance + newPos.y; z++)
        {
            for (int x = -renderDistance + newPos.x; x <= renderDistance + newPos.x; x++)
            {
                Vector2Int pos = new Vector2Int(x, z);

                // Any coordinates that are new, and not part of the old coordinates are added to the load queue
                if (!oldCoords.Contains(pos))
                {
                    loadQueue.Enqueue(pos);
                }

                newCoords.Add(new Vector2Int(x, z));
            }
        }

        // Any coordinates that are old, and not part of the new coordinates are added to the unload queue
        List<Vector2Int> coords = oldCoords.Except(newCoords).ToList();

        foreach (Vector2Int coord in coords)
        {
            unloadQueue.Enqueue(coord);
        }

        // Once the process is over, the old values become the new for later iterations
        oldCoords = new List<Vector2Int>(newCoords);
        oldPos = newPos;
    }

    private Vector2Int playerPosToChunk() // Method that converts a player position to a chunk position using the static int chunkSize
    {
        int xOffset = 0, zOffset = 0;

        if (player.position.x < 0)
        {
            xOffset = -1;
        }

        if (player.position.z < 0)
        {
            zOffset = -1;
        }

        int x = (int)(player.position.x / Chunk.chunkSize) + xOffset;
        int z = (int)(player.position.z / Chunk.chunkSize) + zOffset;

        return new Vector2Int(x, z);
    }
}