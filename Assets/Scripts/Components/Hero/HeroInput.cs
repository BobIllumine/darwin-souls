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
        AddSkill("poisonCloud", gameObject.AddComponent<PoisonCloud>().Initialize(gameObject));


    }
    public override void AddAction(Button button = Button.NO_ACTION)
    {
        queue.Enqueue(new InputAction(button, Time.time, buffer));
    }
    void Update()
    {
        // AddAction(Button.JUMP);
        // buffer.Enqueue(Button.JUMP);
        // var ax = Input.GetAxis(axis);
        if(Input.GetKeyDown((KeyCode)buttons[Button.JUMP]))
            AddAction(Button.JUMP);
            // movementController.Jump();
        
        else if(Input.GetKeyDown((KeyCode)buttons[Button.DEFAULT_ATTACK]))
            AddAction(Button.DEFAULT_ATTACK);
            // actionController.Do("defaultAttack");

        else if(Input.GetKeyDown((KeyCode)buttons[Button.SKILL_1]))
            AddAction(Button.SKILL_1);
            // actionController.Do(skillList[0]);
        
        else if(Input.GetKeyDown((KeyCode)buttons[Button.SKILL_2]))
            AddAction(Button.SKILL_2);
        //     actionController.Do(skillList[1])
        else if(Input.GetKeyDown((KeyCode)buttons[Button.SKILL_3]))
            AddAction(Button.SKILL_3);
        // else
            // AddAction(Button.NO_ACTION, ax);
        movementController.Move(Input.GetAxis(axis));
        // AddAxis(Input.GetAxis(axis));
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
                // if(lastAction.axis != 0f)
                    // movementController.Move(lastAction.axis);
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
        // print($"{name}: {lastAction}, {buffer.Count}");
    }
    public void AddSkill(string name, Action action) 
    {
        actionController.AddAction(name, action);
        skillList.Add(name);
    }
}
