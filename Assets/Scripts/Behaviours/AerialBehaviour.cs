using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AerialBehaviour : StateMachineBehaviour
{
    protected GameObject actor;
    protected BaseState state;
    protected BaseAnimResolver animResolver;    
    protected BaseActionController actionController;
    protected BaseMovementController movementController;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        actor = animator.gameObject;
        state = actor.GetComponent<BaseState>();
        animResolver = actor.GetComponent<BaseAnimResolver>();
        actionController = actor.GetComponent<BaseActionController>();
        movementController = actor.GetComponent<BaseMovementController>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        state.busy = true;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        state.busy = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    // override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //    Debug.Log(movementController.isGrounded);
    // }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
