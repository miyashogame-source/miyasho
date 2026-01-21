using UnityEngine;

public class PoseState : StateMachineBehaviour
{
    override public void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetBool("IsPose", true);
    }
}
