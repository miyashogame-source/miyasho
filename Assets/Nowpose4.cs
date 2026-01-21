using UnityEngine;

public class Nowpose4 : StateMachineBehaviour
{
    override public void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetBool("Nowpose4", true);
    }

     override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Nowpose4", false);
    }
}
