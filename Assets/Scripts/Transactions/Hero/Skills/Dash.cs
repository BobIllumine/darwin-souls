using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
            
        PropertyInfo ms = state.GetType().GetProperty("MS");

        state.ApplyChange((ms, state.MS * 5));
        
        StartCoroutine(movementController.ApplyVelocity(new Vector2(state.MS * animResolver.faceTowards, 0), 0.1f, 0));
        
        animResolver.ChangeStatus(status);
        
        state.ApplyChange((ms, state.MS / 5));

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
        this.animResolver = obj.GetComponent<BaseAnimResolver>();
        this.state = obj.GetComponent<BaseState>();
        this.movementController = obj.GetComponent<BaseMovementController>();
        return this;
    }
}
