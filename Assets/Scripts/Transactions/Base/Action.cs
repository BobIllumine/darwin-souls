using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Action : MonoBehaviour
{
    public BaseState state {get; protected set; }
    public bool isAvailable { get; protected set; }
    public float cooldown { get; protected set; }
    public ActionStatus status { get; protected set; }
    public BaseAnimResolver animResolver { get; protected set; }
    public float cr { get; protected set; }
    public abstract void Fire(float cr);
    public abstract void UseOnState(BaseState state, float cr);

    protected virtual IEnumerator StartCooldown(float cr) 
    {
        isAvailable = false;
    
        yield return new WaitForSeconds(cooldown * (1 - cr));

        isAvailable = true;
    }

    public abstract Action Initialize(GameObject obj);

    public virtual void Serialize() 
    {
        Type thisClass = this.GetType();
        Type[] allInterfaces = thisClass.GetInterfaces();
        // print(String.Join(",", allInterfaces.Select(i=>i.Name)));
    }
}
