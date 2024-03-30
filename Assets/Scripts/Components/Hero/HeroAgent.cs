using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.InputSystem;


public class HeroAgent : BaseAgent
{
    private HeroState state;
    private BaseState oppState;

    public override void ResetParameters()
    {
        base.ResetParameters();
        state.ApplyChanges(initialStats);
    }
    public override void Initialize()
    {
        state = GetComponent<HeroState>();
        oppState = opponent.GetComponent<BaseState>();
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        initialPos = transform.position;
        initialStats = state.stats;
        // ResetParameters();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log(CompletedEpisodes);
        ResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(new List<float>() { oppState.stats.HP, oppState.stats.MS, oppState.stats.AD, oppState.stats.AS });
        sensor.AddObservation(new List<float>() { state.stats.HP, state.stats.MS, state.stats.AD, state.stats.AS });
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float axisX = Mathf.Clamp(actions.ContinuousActions[0], -1, 1);
        movementController.Move(axisX);
        int action = actions.DiscreteActions[0];
        switch(action)
        {
            case (int)Button.JUMP:
                movementController.Jump();
                AddReward(-10f);
                break;
            case (int)Button.DEFAULT_ATTACK:
                actionController.Do("defaultAttack");
                AddReward(-0.1f);
                break;
            default:
                break;
        }
        if(oppState.stats.HP == 0)
        {
            AddReward(100f);
            EndEpisode(); 
        }
        if(state.stats.HP == 0) 
        {
            AddReward(-100f);
            EndEpisode();
        }
        // Debug.Log(GetCumulativeReward());
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }
}
