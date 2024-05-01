using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using UnityEngine.InputSystem;
using Unity.VisualScripting;


public class HeroAgent : BaseAgent
{
    private HeroState state;
    private BaseState oppState;
    private BossMovementController oppMovementController;
    private Vector3 oppInitialPos;
    private Quaternion oppInitialRot;
    private Stats oppInitialStats;

    public override void ResetParameters()
    {
        // print(gameObject.name);
        movementController.Teleport(initialPos);
        oppMovementController.Teleport(oppInitialPos);
        // print($"im still teleporting bitch {initialStats.HP}");
        // initialStats.HP = 100;
        state.ApplyChanges(initialStats);
        oppState.ApplyChanges(oppInitialStats);
        // if(gameObject)
        //     Destroy(gameObject);
        // Instantiate(self, initialPos, initialRot);
    }
    public override void Initialize()
    {
        // print("im initial");
        state = GetComponent<HeroState>();
        oppState = opponent.GetComponent<BaseState>();
        input = GetComponent<BaseInput>();
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        oppMovementController = oppState.transform.gameObject.GetComponent<BossMovementController>();
        initialPos = transform.position;
        initialRot = transform.rotation;
        oppInitialPos = oppState.transform.position;
        oppInitialRot = oppState.transform.rotation;
        initialStats = new Stats(state.stats);
        oppInitialStats = new Stats(oppState.stats);
        // print($"{initialStats.MS}, {initialStats.AD}, {initialStats.MaxHP}");
        // print($"{initialPos}, {initialStats}, {name}");
        // ResetParameters();
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log($"ye {CompletedEpisodes}");
        // print(movementController);
        // print(state.stats);
        ResetParameters();
        // print("huh?");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(new List<float>() { oppState.stats.HP, oppState.stats.MS, oppState.stats.AD, oppState.stats.AS });
        // sensor.AddObservation(new List<float>() { state.stats.HP, state.stats.MS, state.stats.AD, state.stats.AS });
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // print(actions.ContinuousActions[0]);
        int move = actions.DiscreteActions[0];
        switch(move)
        {
            case 1: 
                // print("im trying to move");
                movementController.Move(-1);
                break;
            case 2:
                // print("im trying to move");
                movementController.Move(1);
                break;
            case 3:
                input.AddAction(Button.JUMP);
                break;
            case 4:
                input.AddAction(Button.DEFAULT_ATTACK);
                break;
            default:
                break;
        }
        // print($"move: {move}");
        if(oppState.stats.HP == 0)
        {
            AddReward(oppState.stats.MaxHP * 2);
            EndEpisode(); 
        }
        // Debug.Log(GetCumulativeReward());
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }
}
