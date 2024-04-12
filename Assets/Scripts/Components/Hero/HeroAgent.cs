using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using UnityEngine.InputSystem;


public class HeroAgent : BaseAgent
{
    private HeroState state;
    private BaseState oppState;

    public override void ResetParameters()
    {
        // print(gameObject.name);
        movementController.Teleport(initialPos);
        state.ApplyChanges(initialStats);
        // if(gameObject)
        //     Destroy(gameObject);
        // Instantiate(self, initialPos, initialRot);
    }
    public override void Initialize()
    {
        state = GetComponent<HeroState>();
        oppState = opponent.GetComponent<BaseState>();
        input = GetComponent<BaseInput>();
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        initialPos = transform.position;
        initialRot = transform.rotation;
        initialStats = state.stats;
        // print($"{initialPos}, {initialStats}, {name}");
        // ResetParameters();
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log(CompletedEpisodes);
        ResetParameters();
        Initialize();
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
        print($"axis: {axisX}, action: {action}");
        switch(action)
        {
            case 1:
                input.AddButton(Button.JUMP);
                AddReward(-10f);
                break;
            case 2:
                input.AddButton(Button.DEFAULT_ATTACK);
                AddReward(-0.1f);
                break;
            default:
                print("wtf");
                break;
        }
        if(oppState.stats.HP == 0)
        {
            AddReward(100f);
            EndEpisode(); 
        }
        // Debug.Log(GetCumulativeReward());
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }
}
