using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// [RequireComponent(typeof(BossState))]
// [RequireComponent(typeof(BossAnimResolver))]
// [RequireComponent(typeof(BossMovementController))]
public class DummyActionController : BaseActionController
{
    void Awake()
    {
        state = GetComponent<BossState>();
        animResolver = GetComponent<BossAnimResolver>();
        movementController = GetComponent<BossMovementController>();
        isActionable = true;
        canAttack = true;
        canCast = true;

        actionSpace = new Dictionary<string, Action>();
    }
    public override void Do(string name)
    {
        if(!isActionable || (!canCast && name != "DefaultAttack") || (!canAttack && name == "DefaultAttack"))
            return;
        try
        {
            activeAction = actionSpace[name];
            state.busy = true;
            activeAction.Fire(state.stats.CR);
        }
        catch
        {
            // print(e);
            state.busy = false;
            // Debug.Log("bad luck kiddo");
            return;    
        }
    }
}
