// using System.Collections;
// using System.Collections.Generic;
// using Google.Protobuf.Reflection;
// using UnityEngine;

// [RequireComponent(typeof(BossState))]
// [RequireComponent(typeof(BossAnimResolver))]
// [RequireComponent(typeof(BossActionController))]
// public class BossMovementController : BaseMovementController
// {
//     private Rigidbody2D body;
//     private float _startScale = 5;
//     private float _fallScale = 8;
//     private bool isDashing;
    
//     void Start() 
//     {
//         state = GetComponent<BossState>();
//         animResolver = GetComponent<BossAnimResolver>();
//         body = GetComponent<Rigidbody2D>();
//         actionController = GetComponent<BossActionController>();
//         isGrounded = true;
//         isMovable = true;
//         isDashing = false;
//     }
//     public override int[] isMoving()
//     {
//         throw new System.NotImplementedException();
//     }
//     public override void Stop()
//     {
//         throw new System.NotImplementedException();
//     }

//     void Update() 
//     {
//         // if(Mathf.Abs(body.velocity.x) <= 1e-7f && isGrounded) 
//         // {
//         //     animResolver.AnimateIdle(ActionStatus.IDLE);
//         //     actionController.canAttack = (state.status == Status.STUNNED || state.status == Status.DISARMED) ? false : true;
//         // }
//         // if(Mathf.Abs(body.velocity.x) > 1e-7f && isGrounded)
//         // {
//         //     animResolver.AnimateBool(ActionStatus.RUN, true);
//         //     actionController.canAttack = false;
//         // }
//         // if(body.velocity.y > 1e-7f && !isGrounded)
//         // {
//         //     animResolver.AnimateBool(ActionStatus.JUMP, true);
//         //     actionController.canAttack = false;
//         // }
//         // if(body.velocity.y < -1e-7f && !isGrounded) 
//         // {
//         //     animResolver.AnimateBool(ActionStatus.FALL, true);
//         //     actionController.canAttack = false;
//         // }
//     }
//     void FixedUpdate() 
//     {
//         if(isDashing || !isMovable)
//             return;
//         body.velocity = new Vector2(direction.x * state.MS * Time.fixedDeltaTime, body.velocity.y);
//         if(body.velocity.y < 0)
//             body.gravityScale = _fallScale;
//     }
//     public override void Jump() 
//     {
//         if(!isGrounded || !isMovable)
//             return;
//         body.gravityScale = _startScale;
//         float jumpHeight = Mathf.Sqrt(state.MS) / 4;
//         float jumpForce = Mathf.Sqrt(jumpHeight * (Physics2D.gravity.y * body.gravityScale) * -2) * body.mass;
//         body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
//         isGrounded = false;
//     }

//     public override void Move(float dir)
//     {
//         direction = Vector2.right * dir;
//         if(dir > 0)
//             animResolver.ChangeFacedDirection(1);
//         else if(dir < 0)
//             animResolver.ChangeFacedDirection(-1);
//     }

//     public override void ApplyForce(Vector2 force, ForceMode2D mode)
//     {
//         body.AddForce(force, mode);
//     }

//     public override IEnumerator ApplyVelocity(Vector2 velocity, float duration, float gravity = 0)
//     {   
//         isDashing = true;
        
//         var gravityScale = body.gravityScale;
        
//         body.gravityScale = 0;

//         body.velocity = new Vector2(velocity.x * Time.fixedDeltaTime, velocity.y * Time.fixedDeltaTime);

//         yield return new WaitForSeconds(duration);

//         body.gravityScale = gravityScale;
        
//         isDashing = false;
//     }

//     private void OnCollisionEnter2D(Collision2D other)
//     {
//         isGrounded = other.gameObject.layer == 3 || other.gameObject.layer == 7;
//         if(other.gameObject.layer == 7) 
//         {
//             var x = body.velocity.x;
//             var y = body.velocity.y;
//             body.velocity = Vector2.zero;
//         }
            
//     }
//     private void OnCollisionExit2D(Collision2D other)
//     {
//         isGrounded = !(other.gameObject.layer == 3 || other.gameObject.layer == 7);
//     }
// }
