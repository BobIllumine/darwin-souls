using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.Analytics;

// [RequireComponent(typeof(BossMovementController))]
// [RequireComponent(typeof(BossActionController))]
public class DummyInput : BaseInput
{
    void Start()
    {
        actionController = GetComponent<DummyActionController>();
        movementController = GetComponent<DummyMovementController>();
        skillManager = GetComponent<DummySkillManager>();
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
    public override void BufferButton(Button button)
    {
        queue.Enqueue(new InputAction(button, Time.time, buffer));
    }

    void Update()
    {
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
                    default:
                        break;
                }
                break;
            }
        }
    }
}
