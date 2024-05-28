using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Rendering;
using UnityEngine;

public class Dash : Action, IMobility
{
    public BaseMovementController movementController { get; protected set; }

    public ParticleSystem partSystem { get; protected set; }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable || !movementController.isMovable)
        {
            state.busy = false;
            return;
        }
            
        var emission = partSystem.emission;
        emission.enabled = true;

        Stats newStats = new Stats(state.stats);
        newStats.MS *= 5;
        state.ApplyChanges(newStats);
        
        StartCoroutine(movementController.ApplyVelocity(new Vector2(state.stats.MS * animResolver.faceTowards, 0), 0.1f, 0));
        
        animResolver.ChangeStatus(status);
        
        newStats.MS /= 5;
        state.ApplyChanges(newStats);
        partSystem.Play();

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
        status = ActionStatus.RUN;
        cooldown = 2;
    }
    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        movementController = obj.GetComponent<BaseMovementController>();
        GameObject partObj = Instantiate(Resources.Load<GameObject>("Prefabs/FX/Dash Effect"));
        partObj.transform.SetParent(transform, false);
        partSystem = partObj.GetComponent<ParticleSystem>();

        return this;
    }

    public override float[] Serialize()
    {
        float[] row = Mappings.DefaultSkillRow;
        row[1] = (isAvailable ? 1f : 0f);
        return row; 
    }
}
