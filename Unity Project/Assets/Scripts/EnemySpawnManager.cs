using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    // This script manages the enemy spawns at playtime
    [SerializeField] GameObject[] enemyFishes; // Types of enemy fishes (two in this case)
    [SerializeField] PlayerController pc; // Player's script ref

    [SerializeField] int totalSpawns; // Total number of spawns, (50 in this case)
    public int difficulty;

    private void Awake() // Runs at load time, sets difficulty and spawns all of the fish
    {
        difficulty = pc.score / 30;

        if (difficulty < 1)
        {
            difficulty = 1;
        }

        for (int i = 0; i < totalSpawns; i++)
        {
            Spawn();
        }
    }

    private void Spawn() // Spawn behaviour of fish
    {
        int randomSize = Random.Range(pc.score - 4, pc.score + 3 + difficulty); // Size of fish, more likely to be bigger with harder difficulties
        int directionX = 1, directionZ = 1;

        // Randomizes direction in the X and Z direction (Y is always the same range)
        if (Random.Range(0, 2) == 1)
        {
            directionX = -1;
        }

        if (Random.Range(0, 2) == 1)
        {
            directionZ = -1;
        }

        // Random position is created
        float randomX = Random.Range(pc.transform.position.x + directionX * 75, pc.transform.position.x + directionX * 50);
        float randomY = Random.Range(-95f, -45);
        float randomZ = Random.Range(pc.transform.position.z + directionZ * 75, pc.transform.position.z + directionZ * 50);

        Vector3 newPos = new Vector3(randomX, randomY, randomZ);

        // Instantiates a fish based on previous quantities
        GameObject newFish = Instantiate(enemyFishes[Random.Range(0, enemyFishes.Length)], newPos, Quaternion.identity);
        newFish.GetComponent<EnemyBehaviour>().size = randomSize; // Sets fish size
    }

    public void UpdateFish() // Whenever an enemy fish dies, difficulty is adjusted and a fish is spawned again
    {
        difficulty = pc.score / 30; // Trunucation means that difficulty increments with every 30 size increases
        Spawn();
    }
}