using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// [RequireComponent(typeof(HeroState))]
// [RequireComponent(typeof(HeroAnimResolver))]
// [RequireComponent(typeof(HeroMovementController))]
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
            Stats newStats = state.stats;
            newStats.status = Status.STUNNED;
            state.ApplyChanges(newStats);
            activeAction.Fire(state.stats.CR);
        }
        catch(KeyNotFoundException e)
        {
            print(e);
            Stats newStats = state.stats;
            newStats.status = Status.OK;
            state.ApplyChanges(newStats);
            // Debug.Log("bad luck kiddo");
            return;    
        }
    }
}
