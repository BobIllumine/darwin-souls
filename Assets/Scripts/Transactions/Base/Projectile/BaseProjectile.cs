using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ProjectileAnimResolver))]
public abstract class BaseProjectile : MonoBehaviour
{
    public Vector2 direction { get; protected set; }
    public Vector2 velocity { get; protected set; }
    public Rigidbody2D body { get; protected set; }
    public ProjectileAnimResolver animResolver { get; protected set; }
    public abstract BaseProjectile Initialize(Vector2 direction, Vector2 velocity);

}
