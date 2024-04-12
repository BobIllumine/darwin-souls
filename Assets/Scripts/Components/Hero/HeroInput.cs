using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

[RequireComponent(typeof(HeroMovementController))]
[RequireComponent(typeof(HeroActionController))]
public class HeroInput : BaseInput
{
    void Start()
    {
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        buffer = new LimitedQueue<Button>(5);
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
    public override void AddButton(Button button)
    {
        buffer.Enqueue(button);
    }
    void Update()
    {
        // buffer.Enqueue(Button.JUMP);
        if(Input.GetKeyDown(buttons[Button.JUMP]))
            buffer.Enqueue(Button.JUMP);
        
        if(Input.GetKeyDown(buttons[Button.DEFAULT_ATTACK]))
            buffer.Enqueue(Button.DEFAULT_ATTACK);

        if(Input.GetKeyDown(buttons[Button.SKILL_1]))
            actionController.Do(skillList[0]);
        
        if(Input.GetKeyDown(buttons[Button.SKILL_2]))
            actionController.Do(skillList[1]);
        
        movementController.Move(Input.GetAxis(axis));
    }
    void FixedUpdate()
    {
        Button lastAction;
        if(!buffer.TryDequeue(out lastAction))
            return;
        switch(lastAction)
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

        // print($"{name}: {lastAction}, {buffer.Count}");
    }
    public void AddSkill(string name, Action action) 
    {
        actionController.AddAction(name, action);
        skillList.Add(name);
    }
}
