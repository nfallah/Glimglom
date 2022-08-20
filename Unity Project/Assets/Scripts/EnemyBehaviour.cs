using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    // The script attached to each enemy fish
    public int size;
    private PlayerController pc;
    [SerializeField] Transform readjustments;
    [SerializeField] CharacterController cc;
    [SerializeField] TextMesh score;
    [SerializeField] Animator animator;

    private bool newRotation;
    private float movementSpeed;
    private float defaultXSmooth, defaultYSmooth;
    private float defaultSmoothTime = 0.125f;

    // When an enemy fish is created, it is assigned a random size. Based on this size, scale, speed, the score text and other values are determined
    // Furthermore, the fish begins traveling in a random direction
    private void Start()
    {
        transform.localScale += Vector3.one * (0.02f * size);
        movementSpeed = Mathf.Clamp(6 + 0.005f * (-size + FindObjectOfType<EnemySpawnManager>().difficulty), 4, 10);
        score.text = size.ToString();
        cc.detectCollisions = true;
        pc = FindObjectOfType<PlayerController>();
        transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0); 
        score.GetComponent<FollowRotation>().centerTransform = pc.center; // The score will always rotate to the player in the scene for convenience
        animator.SetFloat("SizeScale", Mathf.Clamp(1 / transform.localScale.x, movementSpeed / 6, 1));
    }

    // Runs if two controller colliders were hit
    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (collision.transform.gameObject == pc.gameObject) // Checks to see that it was the player and not another enemy fish
        {
            // Determines whether the player or enemy "dies" based on the size difference
            if (pc.score >= size) // Player is bigger or the same size, player wins and grows
            {
                pc.Grow();
                Destroy(gameObject); // Fish is destroyed
                FindObjectOfType<EnemySpawnManager>().UpdateFish(); // To make up for one dead fish, another one instantly spawns
            }

            else // Player is smaller, game ends
            {
                FindObjectOfType<GameOver>().ShowText(); // Displays game over screen
                pc.enabled = false; // Disables player controller
                Time.timeScale = 0; // Game "freezes"
            }
        }
    }

    private void Update()
    {
        float disToPlayer = (pc.transform.position - transform.position).magnitude;


        if (disToPlayer >= 125)
        {
            Destroy(gameObject);
            FindObjectOfType<EnemySpawnManager>().UpdateFish();
        }

        else if (disToPlayer <= 17.5f)
        {
            newRotation = true;

            int direction = 1;

            if (size <= pc.score)
            {
                direction = -1;
            }

            Vector3 targetAngle = Quaternion.LookRotation(direction * (pc.transform.position - transform.position)).eulerAngles;
            Vector3 currentAngle = readjustments.eulerAngles;
            float x = Mathf.SmoothDampAngle(currentAngle.x, targetAngle.x, ref defaultXSmooth, defaultSmoothTime);
            float y = Mathf.SmoothDampAngle(currentAngle.y, targetAngle.y, ref defaultYSmooth, defaultSmoothTime);
            readjustments.eulerAngles = new Vector3(x, y, 0);
            cc.Move(readjustments.forward * movementSpeed * Time.deltaTime);
        }

        else
        {
            if (newRotation)
            {
                transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
                readjustments.localEulerAngles = Vector3.zero;
                newRotation = false;
            }

            cc.Move(transform.forward * movementSpeed * Time.deltaTime);
        }
    }
}