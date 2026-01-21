using UnityEngine;

public class Nowpose5 : StateMachineBehaviour
{
    override public void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetBool("Nowpose5", true);
    }
    
     override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Nowpose5", false);
    }
}
