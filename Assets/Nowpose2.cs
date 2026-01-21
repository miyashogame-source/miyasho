using UnityEngine;

public class Nowpose2 : StateMachineBehaviour
{
    override public void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetBool("Nowpose2", true);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Nowpose2", false);
    }
}
