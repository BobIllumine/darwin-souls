using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public abstract class BaseActionController : MonoBehaviour
{
    public Dictionary<string, Action> actionSpace { get; protected set; }
    public bool isActionable { get; set; }
    public bool canAttack { get; set; }
    public bool canCast { get; set; }
    protected Action activeAction;
    public BaseMovementController movementController { get; protected set; }
    public BaseState state { get; protected set; }
    public BaseAnimResolver animResolver { get; protected set; }
    public abstract void Do(string name);
    public virtual void AddAction(string name, Action action) 
    {
        actionSpace.Add(name, action);
    }
}
