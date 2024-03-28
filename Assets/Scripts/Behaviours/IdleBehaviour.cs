using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class IdleBehaviour : BaseBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animResolver = actor.GetComponent<BaseAnimResolver>();
        actionController = actor.GetComponent<BaseActionController>();
        movementController = actor.GetComponent<BaseMovementController>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch(movementController.isGrounded)
        {
            case true:
                if(Mathf.Abs(movementController.velocity.x) >= 1e-3f)
                    animResolver.ChangeStatus(ActionStatus.RUN);
                else
                    animResolver.ChangeStatus(ActionStatus.IDLE);
                break;
            case false:
                if(movementController.velocity.y >= 0f)
                    animResolver.ChangeStatus(ActionStatus.JUMP);
                else
                    animResolver.ChangeStatus(ActionStatus.FALL);
                break;
        }
        
        base.OnStateUpdate(animator, stateInfo, layerIndex);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    // override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //    // Implement code that processes and affects root motion
    //    Debug.Log(movementController.isGrounded);
    // }

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
