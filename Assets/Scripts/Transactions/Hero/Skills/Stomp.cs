using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Stomp : Action, IMobility
{
    public BaseMovementController movementController { get; protected set; }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable)
        {
            state.busy = false;
            return;
        }
            
        Stats newStats = new Stats(state.stats);
        newStats.MS *= 10;
        state.ApplyChanges(newStats);

        
        StartCoroutine(movementController.ApplyVelocity(new Vector2(0, -state.stats.MS), 0.1f, 0));
        
        animResolver.ChangeStatus(status);
        
        newStats.MS /= 10;
        state.ApplyChanges(newStats);
        state.busy = false;
        StartCoroutine(StartCooldown(cr));
    }
    public override void UseOnState(BaseState state, float cr)
    {
        return;
    }
    void Awake() 
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

    public override float[] Serialize()
    {
        float[] row = Mappings.DefaultSkillRow;
        row[1] = (isAvailable ? 1f : 0f);
        return row; 
    }
}
