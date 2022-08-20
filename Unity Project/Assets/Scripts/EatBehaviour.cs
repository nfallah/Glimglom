using UnityEngine;

public class EatBehaviour : StateMachineBehaviour
{
    // Attached to the fish animator, and pretty much helped play the eating animation every time something was eaten
    // (as long as the animation was not already playing)
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 1)
        {
            animator.SetBool("isEating", false);
        }
    }
}