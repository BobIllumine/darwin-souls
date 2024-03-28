using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HeroMovementController))]
[RequireComponent(typeof(HeroActionController))]
public class HeroInput : BaseInput
{
    void Start()
    {
        actionController = GetComponent<HeroActionController>();
        movementController = GetComponent<HeroMovementController>();
        
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
    void Update()
    {
        if(Input.GetKeyDown(buttons[Button.JUMP]))
            movementController.Jump();
        
        if(Input.GetKeyDown(buttons[Button.DEFAULT_ATTACK]))
            actionController.Do("defaultAttack");

        if(Input.GetKeyDown(buttons[Button.SKILL_1]))
            actionController.Do(skillList[0]);
        
        if(Input.GetKeyDown(buttons[Button.SKILL_2]))
            actionController.Do(skillList[1]);

        movementController.Move(Input.GetAxis(axis));
    }
    public void AddSkill(string name, Action action) 
    {
        actionController.AddAction(name, action);
        skillList.Add(name);
    }
}
