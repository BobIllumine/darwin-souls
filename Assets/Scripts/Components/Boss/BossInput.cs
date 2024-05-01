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
    void Start()
    {
        actionController = GetComponent<BossActionController>();
        movementController = GetComponent<BossMovementController>();
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
        skillList = new List<string>();
    }
    public override void AddAction(Button button)
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
        //     actionController.Do("defaultAttack");

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
                        actionController.Do("defaultAttack");
                        break;
                    default:
                        break;
                }
                break;
            }
        }
        

        // print($"{name}: {lastAction}, {buffer.Count}");
    }
    public void AddSkill(string name, Action action) 
    {
        actionController.AddAction(name, action);
        skillList.Add(name);
    }
}
