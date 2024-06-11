using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;
using Unity.VisualScripting;
// using UnityEditor.ShaderGraph.Internal;


public abstract class BaseState : MonoBehaviour
{
    public Stats stats { get; protected set; }
    public bool busy { get; set; }
    public BaseAgent agent { get; protected set; }
    public BaseActionController actionController { get; protected set; }
    public BaseMovementController movementController { get; protected set; }
    public BaseAnimResolver animResolver { get; protected set; }
    protected (Stats, Stats) lastImpact;
    public abstract (Stats, Stats) GetLastImpact();
    public abstract void ApplyChanges(Stats other);
    public abstract void ApplyTimedChanges(Stats other, float duration);
    public abstract void ApplyPeriodicChanges(Func<BaseState, Stats> statChange, float duration, float period);
    public virtual IEnumerator TimedRevert(Stats copy, float duration) 
    {
        yield return new WaitForSeconds(duration);
        ApplyChanges(copy);
    }

    public virtual IEnumerator PeriodicApply(Func<BaseState, Stats> statChange, float duration, float period) 
    {
        float startTime = Time.time;
        while(Time.time - startTime < duration)
        {
            ApplyChanges(statChange(this));
            yield return new WaitForSeconds(period);
        }
    }
    
    public virtual void OnDeath() 
    {
        animResolver.ChangeStatus(ActionStatus.DIE);
    }
    public virtual void Update() 
    {
        if(stats.HP <= 0)
        {
            OnDeath();
            return;
        }
        if(busy)
        {
            movementController.isMovable = false;
            actionController.isActionable = false;
            return;
        }
        switch(stats.status) {
            case Status.OK:
                actionController.canAttack = true;
                actionController.canCast = true;
                actionController.isActionable = true;
                movementController.isMovable = true;
                break;
            case Status.DISARMED:
                actionController.canAttack = false;
                break;
            case Status.MUTE:
                actionController.canCast = false;
                break;
            case Status.ENSNARED:
                movementController.isMovable = false;
                break;
            case Status.STUNNED:
                movementController.isMovable = false;
                actionController.isActionable = false;
                break;
            default:
                break;
        }
    }

}
