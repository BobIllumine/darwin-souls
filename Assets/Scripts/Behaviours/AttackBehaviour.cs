using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
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
        state.busy = true;
        // state.ApplyChange("status", Status.STUNNED);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Stats newStats = state.stats;
        // newStats.status = Status.STUNNED;
        // state.ApplyChanges(newStats);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        state.busy = false;
        // state.ApplyChange("status", Status.OK);
    }
}
