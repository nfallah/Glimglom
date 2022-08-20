using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public int score; // Player size

    [SerializeField] Animator animator; // Animation

    [SerializeField] CharacterController controller; // Movement with collision detection

    [SerializeField] Color startColor, endColor; // Sprint meter start and end colors (green --> red)

    // Various float values used for different things. . .
    [SerializeField] float moveAcc, sprintCooldown, animAcceleration, cameraDistance, defaultSmoothTime, defaultTimer, fastMove, normMove, rotationSmoothTime, rotationSpeed;

    [SerializeField] Image sprintMeter; // The front image that is scaled up/down to visually display the cooldown

    [SerializeField] KeyCode downKey, sprintKey, upKey; // Keys that trigger certain events determined in the editor

    [SerializeField] Text sprintText; // The text that displays the cooldown of sprinting

    [SerializeField] TextMesh scoreMesh; // The text that displays the score of the player

    [SerializeField] Transform fish, fishCameras; // Transforms of the fish and the camera of the fish, used for movement

    public Transform center;  // Center transform (kind of like the camera arm)

    private int direction; // Sprinting acceleration direction

    // Several boolean values used as conditionals for iteration
    private bool cooldownEnabled, isSprinting, shouldDefault;

    // Used for animations, sprinting, etc. . .
    private float sprintTimer, sprintMoveTimer, animVelocity, initialMoveSpeed, movementSpeed, currentDefaultTimer, defaultSmoothVelocityX, sizeScale = 1, turnSmoothVelocityX, turnSmoothVelocityY;

    private Vector3 centerAngle; // Camera arm angle

    private float moveVel; // Control sprint movement, works in tandem with move acceleration

    private void Start() // Sets values declared in inspector
    {
        Cursor.lockState = CursorLockMode.Locked;
        scoreMesh.text = score.ToString();
        initialMoveSpeed = normMove;
        movementSpeed = normMove;
        fishCameras.transform.localPosition = cameraDistance * Vector3.back;
        currentDefaultTimer = defaultTimer;
    }

    private void Update()
    {
        if (Input.GetKeyDown(sprintKey) && !cooldownEnabled) // If you click the sprinting key and it's not on cooldown, things happen
        {
            direction = 1;
            moveVel = 0;
            cooldownEnabled = true;
            isSprinting = true;
            normMove = movementSpeed;
            movementSpeed = fastMove;
            sprintTimer = sprintCooldown;
            sprintMoveTimer = 0.5f;
            Sprint(); // The cooldown/UI management
            SprintMove(); // The actual movement
        }

        float xRot = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        float yRot = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

        // Sets center/camera angles based on mouse X and mouse Y movement
        centerAngle.x = Mathf.Clamp(centerAngle.x + xRot, -89.9f, 89.9f);
        centerAngle.y = (centerAngle.y + yRot) % 360;
        center.eulerAngles = centerAngle;

        // Sets movement values based on axis movements and up/down key presses
        float xTranslate = Input.GetAxisRaw("Horizontal");
        float yTranslate = 0;
        float zTranslate = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(downKey))
        {
            yTranslate--;
        }

        if (Input.GetKey(upKey))
        {
            yTranslate++;
        }

        Vector3 moveDir = xTranslate * center.right + yTranslate * transform.up + zTranslate * center.forward;

        if (moveDir.magnitude > 0 && !isSprinting) // Moves and rotates if not sprinting and moving at all
        {
            if (shouldDefault)
            {
                shouldDefault = false;
            }

            if (currentDefaultTimer != defaultTimer) // Idle timer reset
            {
                currentDefaultTimer = defaultTimer;
            }

            Vector3 newRotation = Quaternion.LookRotation(moveDir).eulerAngles; // New angle fish should be facing

            if (xTranslate == 0 && zTranslate == 0) // Makes vertical only movement slightly less iffy
            {
                newRotation.x = -Mathf.Sign(yTranslate) * 89.9f;
                newRotation.y = fish.eulerAngles.y;
            }

            // Smoothly rotates rather than instantaneously
            float angleX = Mathf.SmoothDampAngle(fish.eulerAngles.x, newRotation.x, ref turnSmoothVelocityX, rotationSmoothTime);
            float angleY = Mathf.SmoothDampAngle(fish.eulerAngles.y, newRotation.y, ref turnSmoothVelocityY, rotationSmoothTime);

            // Blends from idle to moving and sets angles/positions
            animVelocity = Mathf.Clamp(animVelocity + animAcceleration * Time.deltaTime, 0, 1);
            fish.eulerAngles = new Vector3(angleX, angleY, 0);
            controller.Move(movementSpeed * Time.deltaTime * fish.forward);
            transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, transform.position.y, -50), transform.position.z);
        }

        else if (!isSprinting) // Runs timer, which when reaching zero, defaults the rotation of the fish
        {
            animVelocity = Mathf.Clamp(animVelocity - animAcceleration * Time.deltaTime, 0, 1);

            if (!shouldDefault && currentDefaultTimer != 0)
            {
                currentDefaultTimer = Mathf.Clamp(currentDefaultTimer - Time.deltaTime, 0, defaultTimer);
            }

            else if (!shouldDefault)
            {
                shouldDefault = true;
            }

            else if (fish.eulerAngles != new Vector3(0, fish.eulerAngles.y, 0))
            {
                float x = Mathf.SmoothDampAngle(fish.eulerAngles.x, 0, ref defaultSmoothVelocityX, defaultSmoothTime);

                fish.eulerAngles = new Vector3(x, fish.eulerAngles.y, 0);
            }
        }

        animator.SetFloat("Velocity", animVelocity); // Idle --> moving or moving --> idle based on whether you moved or not this frame
    }

    private void SprintMove() // Sprint movement managed,  uses recursion
    {
        sprintMoveTimer = Mathf.Clamp(sprintMoveTimer - Time.deltaTime, 0, 0.5f);

        if (sprintMoveTimer != 0)
        {
            moveVel += moveAcc * direction;
            controller.Move(movementSpeed * moveVel * Time.deltaTime * fish.forward);
            animVelocity = Mathf.Clamp(animVelocity + animAcceleration * 30 * Time.deltaTime, 0, 1);
            transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, transform.position.y, -50), transform.position.z);
            Invoke("SprintMove", Time.deltaTime);
        }

        else if (direction == 1)
        {
            direction = -1;
            sprintMoveTimer = 0.5f;
            SprintMove();
        }

        else
        {
            movementSpeed = normMove;
            isSprinting = false;
        }
    }

    private void Sprint() // Sprint cooldown management, uses recursion
    {
        sprintTimer = Mathf.Clamp(sprintTimer - Time.deltaTime, 0, sprintCooldown);
        Color newColor = Color.Lerp(startColor, endColor, sprintTimer / sprintCooldown);
        sprintMeter.transform.localScale = new Vector3((1 - sprintTimer / sprintCooldown), 1, 1);
        sprintMeter.color = newColor;
        
        if (sprintTimer == 0)
        {
            sprintText.text = "Sprint ready (Q)";
            cooldownEnabled = false;
        }

        else
        {
            sprintText.text = "Sprint ready in " + ((int)(sprintTimer + 1)).ToString() + "...";
            Invoke("Sprint", Time.deltaTime);
        }
    }

    public void Grow() // Runs when the fish grows, and sets values of the fish based on the new size
    {
        animator.SetBool("isEating", true);
        transform.localScale += 0.02f * Vector3.one;
        fishCameras.transform.localPosition += 0.0075f * Vector3.back;
        rotationSmoothTime = Mathf.Clamp(rotationSmoothTime + 0.0008f, rotationSmoothTime, 0.5f);

        if (isSprinting)
        {
            normMove = Mathf.Clamp(normMove - 0.005f, 4, normMove);
            sizeScale = Mathf.Clamp(1 / transform.localScale.x, normMove / initialMoveSpeed, sizeScale);
        }

        else
        {
            movementSpeed = Mathf.Clamp(movementSpeed - 0.005f, 4, movementSpeed);
            sizeScale = Mathf.Clamp(1 / transform.localScale.x, movementSpeed / initialMoveSpeed, sizeScale);
        }

        animator.SetFloat("SizeScale", sizeScale);
        score++;
        scoreMesh.text = score.ToString();
    }
}