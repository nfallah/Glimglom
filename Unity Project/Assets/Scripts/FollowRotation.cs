using UnityEngine;

public class FollowRotation : MonoBehaviour
{
    public Transform centerTransform;

    private void Update() // Based on a transform, the script has the same angles as that transform (used for the scores facing the camera)
    {
        transform.eulerAngles = centerTransform.eulerAngles.y * Vector3.up;
    }
}