using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.Analytics;

// [RequireComponent(typeof(BossMovementController))]
// [RequireComponent(typeof(BossActionController))]
public class BossInput : BaseInput
{
    public BossAgent agent { get; protected set; }
    void Awake()
    {
        queue = new LimitedQueue<InputAction>(20);
        buffer = Time.fixedDeltaTime * 30;
        if(player == Player.P1)
        {
            buttons = Mappings.DefaultInputMapP1;
            axis = "Horizontal_P1";   
        }
        else
        {
            buttons = Mappings.DefaultInputMapP2;
            axis = "Horizontal_P2";
        }
    }
    void Start()
    {
        actionController = GetComponent<BossActionController>();
        movementController = GetComponent<BossMovementController>();
        skillManager = GetComponent<BossSkillManager>();
        agent = GetComponent<BossAgent>();
    }
    public override void BufferButton(Button button)
    {
        queue.Enqueue(new InputAction(button, Time.time, buffer));
    }

    void Update()
    {
        // buffer.Enqueue(Button.JUMP);
        // if(Input.GetKeyDown(buttons[Button.JUMP]))
        //     // AddAction(Button.JUMP);
        //     movementController.Jump();
        
        // if(Input.GetKeyDown(buttons[Button.DEFAULT_ATTACK]))
        //     // AddAction(Button.DEFAULT_ATTACK);
        //     actionController.Do("DefaultAttack");

        // // if(Input.GetKeyDown(buttons[Button.SKILL_1]))
        // //     actionController.Do(skillList[0]);
        
        // // if(Input.GetKeyDown(buttons[Button.SKILL_2]))
        // //     actionController.Do(skillList[1]);
        
        // movementController.Move(Input.GetAxis(axis));
    }
    void FixedUpdate()
    {
        while(queue.Count > 0) 
        {
            InputAction lastAction;
            if(!queue.TryDequeue(out lastAction))
                return;
            if(lastAction.Validate())
            {
                string skill = "";
                switch(lastAction.button)
                {
                    case Button.JUMP:
                        movementController.Jump();
                        agent.AddReward(0.02f);
                        break;
                    case Button.DEFAULT_ATTACK:
                        actionController.Do("DefaultAttack");
                        agent.AddReward(0.02f);
                        break;
                    case Button.SKILL_1:
                        skill = skillManager.GetSkill(0);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_2:
                        skill = skillManager.GetSkill(1);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_3:
                        skill = skillManager.GetSkill(2);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_4:
                        skill = skillManager.GetSkill(3);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_5:
                        skill = skillManager.GetSkill(4);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_6:
                        skill = skillManager.GetSkill(5);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_7:
                        skill = skillManager.GetSkill(6);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_8:
                        skill = skillManager.GetSkill(7);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    case Button.SKILL_9:
                        skill = skillManager.GetSkill(8);
                        if(skill != "null")
                        {
                            actionController.Do(skill);
                            agent.AddReward(0.01f);
                        }
                        break;
                    default:
                        skill = "";
                        break;
                }
                break;
            }
        }
        

        // print($"{name}: {lastAction}, {buffer.Count}");
    }
}
