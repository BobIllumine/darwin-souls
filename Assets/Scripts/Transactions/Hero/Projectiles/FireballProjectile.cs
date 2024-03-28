using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class FireballProjectile : BaseProjectile
{
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animResolver = GetComponent<ProjectileAnimResolver>();
        animResolver.AnimateTrigger(ProjectileStatus.CAST);
    }
    void FixedUpdate() {
        body.velocity = velocity;    
    }
    void OnCollisionEnter2D(Collision2D other) {
        animResolver.AnimateTrigger(ProjectileStatus.HIT);
        if((transform.parent.gameObject.CompareTag("Player") && other.gameObject.CompareTag("Enemy")) 
        || transform.parent.gameObject.CompareTag("Enemy") && other.gameObject.CompareTag("Player")) {
            SendMessageUpwards("OnHit", other);
        }
    }
    void DestroyOnHit() {
        Destroy(gameObject);
    }
    public override BaseProjectile Initialize(Vector2 direction, Vector2 velocity)
    {
        this.direction = direction;
        this.velocity = velocity;
        return this;
    }

}
