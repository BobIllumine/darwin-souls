using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Reflection;


public abstract class BaseState : MonoBehaviour
{
    private int _curHP, _maxHP; 
    public int HP 
    { 
        get { return _curHP; } 
        protected set { _curHP = value > _maxHP ? _maxHP : value; } 
    }
    public int MaxHP 
    {
        get { return _maxHP; }
        protected set { _maxHP = value; _curHP = _maxHP < _curHP ? _maxHP : _curHP; }
    }

    public BaseActionController actionController { get; protected set; }
    public BaseMovementController movementController { get; protected set; }
    public BaseAnimResolver animResolver { get; protected set; }
    public int AD { get; protected set; }
    public float MS { get; protected set; }
    public float AS { get; protected set; }
    public float CR { get; protected set; }
    public Status status { get; protected set; }
    protected Dictionary<PropertyInfo, (object, object)> lastImpact;
    public abstract Dictionary<PropertyInfo, (object, object)> GetLastImpact();
    public abstract void ApplyChange((PropertyInfo, object) stat);
    public abstract void ApplyChange(string name, object stat);
    public abstract void ApplyChanges(Dictionary<PropertyInfo, object> other);
    public abstract void ApplyTimedChanges(Dictionary<PropertyInfo, object> other, float duration);
    public virtual IEnumerator TimedRevert(Dictionary<PropertyInfo, object> copy, float duration) {
        yield return new WaitForSeconds(duration);
        ApplyChanges(copy);
    }

    public virtual void DestroyOnDeath() 
    {
        Destroy(gameObject);
    }
    protected virtual void Update() 
    {
        if(HP <= 0)
        {
            animResolver.ChangeStatus(ActionStatus.DIE);
            return;
        }

        switch(status) {
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

    protected PropertyInfo[] GetBaseProperties() 
    {
        return typeof(BaseState).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
    }
}
