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
    private BaseMovementController oppMovementController;
    private Vector3 oppInitialPos;
    private Quaternion oppInitialRot;
    private Stats oppInitialStats;
    private GameObject gameManager;

    public override void ResetParameters()
    {
        movementController = GetComponent<HeroMovementController>();
        oppMovementController = opponent.GetComponent<BaseMovementController>();

        gameManager = GameObject.FindGameObjectsWithTag("GameController")[0];
        initialPos = gameManager.GetComponent<GameManager>().GetInitialPosition(gameObject.name);
        initialStats = gameManager.GetComponent<GameManager>().GetInitialStats(gameObject.name);

        oppInitialPos = gameManager.GetComponent<GameManager>().GetInitialPosition(oppState.gameObject.name);
        oppInitialStats = gameManager.GetComponent<GameManager>().GetInitialStats(oppState.gameObject.name);

        if(self.GetComponent<HeroSkillManager>() != null)
        {
            HeroSkillManager skillManager = self.GetComponent<HeroSkillManager>();
            if(skillManager.currentSkills.Count() == Mappings.SkillMap.Count())
                skillManager.RemoveAll();
        }

        movementController.Teleport(initialPos);
        oppMovementController.Teleport(oppInitialPos);
        state.ApplyChanges(initialStats);
        oppState.ApplyChanges(oppInitialStats);
    }

    protected void Start()
    {
        state = GetComponent<HeroState>();
        oppState = opponent.GetComponent<BaseState>();
        input = GetComponent<BaseInput>();
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        oppMovementController = opponent.GetComponent<BaseMovementController>();
        
    }
    // protected override void OnEnable()
    // {
    //     base.OnEnable();
    //     // initialPos = transform.position;
    //     // initialRot = transform.rotation;
    //     // oppInitialPos = oppState.transform.position;
    //     // oppInitialRot = oppState.transform.rotation;
    //     // initialStats = new Stats(state.stats);
    //     // oppInitialStats = new Stats(oppState.stats);
    // }

    public override void OnEpisodeBegin()
    {
        ResetParameters();
        // print($"Reward: {GetCumulativeReward()}");
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
                movementController.Move(-1);
                AddReward(0.03f);
                break;
            case 2:
                movementController.Move(1);
                AddReward(0.03f);
                break;
            case 3:
                input.BufferButton(Button.JUMP);
                break;
            case 4:
                input.BufferButton(Button.DEFAULT_ATTACK);
                break;
            case 5:
                input.BufferButton(Button.SKILL_1);
                break;
            case 6:
                input.BufferButton(Button.SKILL_2);
                break;
            case 7:
                input.BufferButton(Button.SKILL_3);
                break;
            case 8:
                input.BufferButton(Button.SKILL_4);
                break;
            case 9:
                input.BufferButton(Button.SKILL_5);
                break;
            case 10:
                input.BufferButton(Button.SKILL_6);
                break;
            case 11:
                input.BufferButton(Button.SKILL_7);
                break;
            case 12:
                input.BufferButton(Button.SKILL_8);
                break;
            case 13:
                input.BufferButton(Button.SKILL_9);
                break;
            default:
                AddReward(-0.01f);
                break;
        }
        if(oppState.stats.HP == 0)
        {
            AddReward(oppState.stats.MaxHP / 200f);
            EndEpisode(); 
        }
        // print($"Opp HP: {oppState.stats.HP}/{oppState.stats.MaxHP}");
        // Debug.Log(GetCumulativeReward());
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }
}
