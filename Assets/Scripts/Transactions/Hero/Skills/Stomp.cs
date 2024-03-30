using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Stomp : Action, IMobility, IStateDependent
{
    public BaseState state { get; protected set; }
    public BaseMovementController movementController { get; protected set; }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable)
            return;
            
        PropertyInfo ms = state.GetType().GetProperty("MS");

        Stats newStats = state.stats;
        newStats.MS *= 10;
        state.ApplyChanges(newStats);

        
        StartCoroutine(movementController.ApplyVelocity(new Vector2(0, -state.stats.MS), 0.1f, 0));
        
        animResolver.ChangeStatus(status);
        
        newStats.MS *= 10;
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
        status = ActionStatus.FALL;
        cooldown = 3;
    }
    public override Action Initialize(GameObject obj) 
    {
        movementController = obj.GetComponent<BaseMovementController>();
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        return this;
    }
}
