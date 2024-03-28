using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMovementController : MonoBehaviour
{
    public BaseState state { get; protected set; }
    public BaseAnimResolver animResolver { get; protected set; }
    public BaseActionController actionController { get; protected set; }
    public bool isMovable { get; set; }
    public bool isGrounded { get; protected set; }
    public abstract int[] isMoving();
    public Vector2 direction { get; protected set; }
    public Vector2 velocity { get; protected set; }
    public abstract void Jump();
    public abstract void Move(float direction);
    public abstract void Stop();
    public abstract void ApplyForce(Vector2 force, ForceMode2D mode);
    public abstract IEnumerator ApplyVelocity(Vector2 velocity, float duration, float gravity);

}
