using UnityEngine;

public class SuspiciousState : StateMachineBehaviour
{
    override public void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetBool("Suspicious", true);
    }
}
