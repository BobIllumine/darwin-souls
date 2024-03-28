using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(HeroState))]
[RequireComponent(typeof(HeroAnimResolver))]
[RequireComponent(typeof(HeroMovementController))]
public class HeroActionController : BaseActionController
{
    void Awake()
    {
        state = GetComponent<HeroState>();
        animResolver = GetComponent<HeroAnimResolver>();
        movementController = GetComponent<HeroMovementController>();
        isActionable = true;
        canAttack = true;
        canCast = true;

        actionSpace = new Dictionary<string, Action>() {
            ["defaultAttack"] = gameObject.GetComponentInChildren<DefaultAttack>().Initialize(gameObject),
        };
    }
    public override void Do(string name)
    {
        if(!isActionable || (!canCast && name != "defaultAttack") || (!canAttack && name == "defaultAttack"))
            return;
        try
        {
            activeAction = actionSpace[name];
            state.ApplyChange("status", Status.STUNNED);
            activeAction.Fire(state.CR);
        }
        catch(KeyNotFoundException e)
        {
            print(e);
            state.ApplyChange("status", Status.OK);
            Debug.Log("bad luck kiddo");
            return;    
        }
    }
}