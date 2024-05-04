using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.TextCore;

public class Blink : Action, IMobility
{
    public BaseMovementController movementController { get; protected set; }
    public ParticleSystem partSystem { get; protected set; }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable)
        {
            state.busy = false;
            return;
        }
        
        var emission = partSystem.emission;
        emission.enabled = true;
        
        Vector2 newLocation = new Vector2(movementController.transform.position.x + (state.stats.MS * animResolver.faceTowards * Time.fixedDeltaTime * 0.5f), movementController.transform.position.y);
        print(newLocation);
        

        movementController.Teleport(newLocation);

        
        animResolver.ChangeStatus(status);
        
        partSystem.Play();
        state.busy = false;

        StartCoroutine(StartCooldown(cr));
        
        // partSystem.Play();
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
        GameObject partObj = Instantiate(Resources.Load<GameObject>("Prefabs/FX/Blink Effect"));
        partObj.transform.SetParent(transform, false);
        partSystem = partObj.GetComponent<ParticleSystem>();
        return this;
    }
}
