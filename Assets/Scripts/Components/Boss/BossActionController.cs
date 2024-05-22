using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// [RequireComponent(typeof(BossState))]
// [RequireComponent(typeof(BossAnimResolver))]
// [RequireComponent(typeof(BossMovementController))]
public class BossActionController : BaseActionController
{
    void Awake()
    {
        state = GetComponent<BossState>();
        animResolver = GetComponent<BossAnimResolver>();
        movementController = GetComponent<BossMovementController>();
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
            Stats newStats = new Stats(state.stats);
            newStats.status = Status.STUNNED;
            state.ApplyChanges(newStats);
            activeAction.Fire(state.stats.CR);
        }
        catch(KeyNotFoundException e)
        {
            // print(e);
            Stats newStats = new Stats(state.stats);
            newStats.status = Status.OK;
            state.ApplyChanges(newStats);
            // Debug.Log("bad luck kiddo");
            return;    
        }
    }
}
