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
    private GameObject gameManager;

    public override void ResetParameters()
    {
        movementController = GetComponent<HeroMovementController>();

        gameManager = GameObject.FindGameObjectsWithTag("GameController")[0];
        initialPos = gameManager.GetComponent<GameManager>().GetInitialPosition(gameObject.name);
        initialStats = gameManager.GetComponent<GameManager>().GetInitialStats(gameObject.name);

        // if(self.GetComponent<HeroSkillManager>() != null)
        // {
        //     HeroSkillManager skillManager = self.GetComponent<HeroSkillManager>();
        //     if(skillManager.currentSkills.Count() == Mappings.SkillMap.Count())
        //         skillManager.RemoveAll();
        // }

        movementController.Teleport(initialPos);
        state.ApplyChanges(initialStats);
    }

    protected void Start()
    {
        state = GetComponent<HeroState>();
        oppState = opponent.GetComponent<BaseState>();
        input = GetComponent<BaseInput>();
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
    }

    public override void OnEpisodeBegin()
    {
        ResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // sensor.AddObservation(new List<float>() { oppState.stats.HP, oppState.stats.MS, oppState.stats.AD, oppState.stats.AS });
        // sensor.AddObservation(new List<float>() { state.stats.HP, state.stats.MS, state.stats.AD, state.stats.AS });
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if(StepCount == MaxStep)
            SendMessageUpwards("OnMaxStep");
        int move = actions.DiscreteActions[0];
        switch(move)
        {
            case 1: 
                movementController.Move(-1);
                break;
            case 2:
                movementController.Move(1);
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
        if(oppState.stats.HP <= 0)
        {
            AddReward(oppState.stats.MaxHP / 100f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }
}
