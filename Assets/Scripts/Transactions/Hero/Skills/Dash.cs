using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;

public class Dash : Action, IMobility, IStateDependent
{
    public BaseState state { get; protected set; }
    public BaseMovementController movementController { get; protected set; }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable)
            return;
            
        Stats newStats = state.stats;
        newStats.MS *= 5;
        state.ApplyChanges(newStats);

        
        StartCoroutine(movementController.ApplyVelocity(new Vector2(state.stats.MS * animResolver.faceTowards, 0), 0.1f, 0));
        
        animResolver.ChangeStatus(status);
        
        newStats.MS /= 5;
        state.ApplyChanges(newStats);

        StartCoroutine(StartCooldown(cr));
    }
    public override void UseOnState(BaseState state, float cr)
    {
        return;
    }
    void Start() 
    {
        isAvailable = true;
        status = ActionStatus.RUN;
        cooldown = 2;
    }
    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        movementController = obj.GetComponent<BaseMovementController>();
        return this;
    }
}
