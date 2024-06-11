using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(HeroState))]
// [RequireComponent(typeof(HeroAnimResolver))]
// [RequireComponent(typeof(HeroMovementController))]
public class HeroActionController : BaseActionController
{
    void Awake()
    {
        isActionable = true;
        canAttack = true;
        canCast = true;

        actionSpace = new Dictionary<string, Action>();
    }
    void Start()
    {
        state = GetComponent<HeroState>();
        animResolver = GetComponent<HeroAnimResolver>();
        movementController = GetComponent<HeroMovementController>();
        actionSpace["DefaultAttack"] = gameObject.GetComponentInChildren<DefaultAttack>().Initialize(gameObject);
    }
    public override void Do(string name)
    {
        if(!isActionable || (!canCast && name != "DefaultAttack") || (!canAttack && name == "DefaultAttack") || name == "null")
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
