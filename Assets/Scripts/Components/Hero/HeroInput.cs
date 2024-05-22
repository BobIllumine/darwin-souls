using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.Analytics;

// [RequireComponent(typeof(HeroMovementController))]
// [RequireComponent(typeof(HeroActionController))]
public class HeroInput : BaseInput
{
    void Start()
    {
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        queue = new LimitedQueue<InputAction>(20);
        buffer = Time.fixedDeltaTime * 10;
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

        AddSkill("blink", gameObject.AddComponent<Blink>().Initialize(gameObject));
        AddSkill("dash", gameObject.AddComponent<Dash>().Initialize(gameObject));
        AddSkill("sonicWave", gameObject.AddComponent<SonicWave>().Initialize(gameObject));


    }
    public override void BufferButton(Button button = Button.NO_ACTION)
    {
        queue.Enqueue(new InputAction(button, Time.time, buffer));
    }
    void Update()
    {
        if(Input.GetKeyDown((KeyCode)buttons[Button.JUMP]))
            BufferButton(Button.JUMP);

        else if(Input.GetKeyDown((KeyCode)buttons[Button.DEFAULT_ATTACK]))
            BufferButton(Button.DEFAULT_ATTACK);

        else if(Input.GetKeyDown((KeyCode)buttons[Button.SKILL_1]))
            BufferButton(Button.SKILL_1);

        else if(Input.GetKeyDown((KeyCode)buttons[Button.SKILL_2]))
            BufferButton(Button.SKILL_2);

        else if(Input.GetKeyDown((KeyCode)buttons[Button.SKILL_3]))
            BufferButton(Button.SKILL_3);
        movementController.Move(Input.GetAxis(axis));
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
                    case Button.SKILL_1:
                        actionController.Do(skillList[0]);
                        break;
                    case Button.SKILL_2:
                        actionController.Do(skillList[1]);
                        break;
                    case Button.SKILL_3:
                        actionController.Do(skillList[2]);
                        break;
                    default:
                        break;
                }
                break;
            }
        }
    }
    public void AddSkill(string name, Action action) 
    {
        actionController.AddAction(name, action);
        skillList.Add(name);
    }
}
