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
                switch(lastAction.button)
                {
                    case Button.JUMP:
                        movementController.Jump();
                        break;
                    case Button.DEFAULT_ATTACK:
                        actionController.Do("DefaultAttack");
                        break;
                    case Button.SKILL_1:
                        actionController.Do(skillManager.GetSkill(0));
                        break;
                    case Button.SKILL_2:
                        actionController.Do(skillManager.GetSkill(1));
                        break;
                    case Button.SKILL_3:
                        actionController.Do(skillManager.GetSkill(2));
                        break;
                    case Button.SKILL_4:
                        actionController.Do(skillManager.GetSkill(3));
                        break;
                    case Button.SKILL_5:
                        actionController.Do(skillManager.GetSkill(4));
                        break;
                    case Button.SKILL_6:
                        actionController.Do(skillManager.GetSkill(5));
                        break;
                    case Button.SKILL_7:
                        actionController.Do(skillManager.GetSkill(6));
                        break;
                    case Button.SKILL_8:
                        actionController.Do(skillManager.GetSkill(7));
                        break;
                    case Button.SKILL_9:
                        actionController.Do(skillManager.GetSkill(8));
                        break;
                    
                    default:
                        break;
                }
                break;
            }
        }
        

        // print($"{name}: {lastAction}, {buffer.Count}");
    }
}
