using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossMovementController))]
[RequireComponent(typeof(BossActionController))]
public class BossInput : BaseInput
{
    private Dictionary<Button, KeyCode> buttons;
    private List<string> skillList;
    void Start()
    {
        actionController = GetComponent<BossActionController>();
        movementController = GetComponent<BossMovementController>();
        buttons = Mappings.DefaultInputMapP2;
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

        movementController.Move(Input.GetAxis("Horizontal_P2"));
    }
    public void AddSkill(string name, Action action) 
    {
        actionController.AddAction(name, action);
        skillList.Add(name);
    }
}
