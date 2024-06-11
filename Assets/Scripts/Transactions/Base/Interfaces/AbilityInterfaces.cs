using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public interface ITarget 
{
    public ActionStatus targetStatus { get; }
    public BaseAnimResolver targetAnimResolver { get; }
}

public interface IProjectile
{
    public GameObject projectile { get; }
    public void OnHit(Collision2D other);
}



public interface IMobility
{
    public BaseMovementController movementController { get; }
}
