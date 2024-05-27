using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class DummyMovementController : BaseMovementController
{
    private Rigidbody2D body;
    private BoxCollider2D col;
    private Collider2D[] results;
    private ContactFilter2D filter;
    private float _startScale = 5;
    private float _fallScale = 8;
    private bool isDashing;
    // private BaseAgent agent;

    void Start() 
    {
        state = GetComponent<DummyState>();
        animResolver = GetComponent<DummyAnimResolver>();
        col = GetComponent<BoxCollider2D>();
        body = GetComponent<Rigidbody2D>();
        actionController = GetComponent<DummyActionController>();
        results = new Collider2D[10];
        filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Ground"));
        filter.useLayerMask = true;
        isGrounded = true;
        isMovable = true;
        isDashing = false;
    }

    public override int[] isMoving() => new int[2] {(Mathf.Abs(body.velocity.x) > 1e-3f ? 1 : 0), (body.velocity.y < -1e-3f ? -1 : (body.velocity.y > 1e-3f ? 1 : 0))};

    void Update() 
    {
        if(Mathf.Abs(body.velocity.x) <= 1e-3f && isGrounded) 
        {
            animResolver.ChangeStatus(ActionStatus.IDLE);
        }
        else if(Mathf.Abs(body.velocity.x) > 1e-3f && isGrounded)
        {
            animResolver.ChangeStatus(ActionStatus.RUN);
        }
        if(body.velocity.y > 1e-3f && !isGrounded)
        {
            animResolver.ChangeStatus(ActionStatus.JUMP);
        }
        else if(body.velocity.y < -1e-3f && !isGrounded && animResolver.status != ActionStatus.ATTACK) 
        {
            animResolver.ChangeStatus(ActionStatus.FALL);
        }
    }
    void FixedUpdate() 
    {
        if(isDashing || !isMovable)
            return;
        isGrounded = Physics2D.OverlapCollider(col, filter, results) > 0;
        body.velocity = new Vector2(direction.x * state.stats.MS * Time.fixedDeltaTime, body.velocity.y);
        velocity = body.velocity;
        // print($"{name}: {velocity}");
        // print($"{name}: {isGrounded}");
        if(body.velocity.y < 0)
            body.gravityScale = _fallScale;
    }
    public override void Teleport(Vector2 position)
    {
        // body.isKinematic = true;
        var tmpVelocity = velocity;
        body.velocity = Vector2.zero;
        body.Sleep();
        body.position = new Vector2(position.x, position.y);
        // body.angularVelocity = 0f;
        // body.position = position;
        // body.isKinematic = false;
        body.WakeUp();
        body.velocity = tmpVelocity;
        // print(isMovable);
        // Stop();
    }
    public override void Stop()
    {
        body.velocity = Vector2.zero;
        Stats newStats = new Stats(state.stats);
        newStats.status = Status.STUNNED;
        state.ApplyChanges(newStats);
    }
    public override void Jump() 
    {
        isGrounded = Physics2D.OverlapCollider(col, filter, results) > 0;
        if(!isGrounded || !isMovable)
            return;
        body.gravityScale = _startScale;
        float jumpHeight = Mathf.Sqrt(state.stats.MS) / 4;
        float jumpForce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * body.gravityScale) * -2) * body.mass;
        body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public override void Move(float dir)
    {
        direction = Vector2.right * dir;
        // body.velocity = new Vector2(direction.x * state.MS * Time.fixedDeltaTime, body.velocity.y);
        if(dir > 0)
            animResolver.ChangeFacedDirection(1);
        else if(dir < 0)
            animResolver.ChangeFacedDirection(-1);
    }

    public override void ApplyForce(Vector2 force, ForceMode2D mode)
    {
        body.AddForce(force, mode);
    }

    public override IEnumerator ApplyVelocity(Vector2 velocity, float duration, float gravity = 0)
    {   
        isDashing = true;
        
        var gravityScale = body.gravityScale;
        
        body.gravityScale = 0;

        body.velocity = new Vector2(velocity.x * Time.fixedDeltaTime, velocity.y * Time.fixedDeltaTime);

        yield return new WaitForSeconds(duration);

        body.gravityScale = gravityScale;
        
        isDashing = false;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        isGrounded = other.gameObject.layer == 3;
        // if(other.gameObject.layer == 7) 
        // {
        //     isGrounded = false;
        //     animResolver.ChangeStatus(ActionStatus.HURT);
        //     agent.AddReward(-0.1f);
        // }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        isGrounded = !(other.gameObject.layer == 3);
        // if(other.gameObject.layer == 7)
        // {
        //     state.ApplyChange("status", Status.OK);
        //     agent.AddReward(0.1f);
        // }
    }
}
