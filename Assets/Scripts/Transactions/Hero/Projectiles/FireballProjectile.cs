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
        animResolver.ChangeStatus(ProjectileStatus.CAST);
    }
    void FixedUpdate() {
        body.velocity = velocity;    
    }
    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.name != parent.name && other.gameObject.layer == 6) {
            animResolver.ChangeStatus(ProjectileStatus.HIT);
            parent.GetComponent<Fireball>().OnHit(other);
            DestroyOnHit();
        }
    }
    void DestroyOnHit() {
        Destroy(gameObject);
    }
    public override BaseProjectile Initialize(Vector2 direction, Vector2 velocity, GameObject parent)
    {
        this.direction = direction;
        this.velocity = velocity;
        this.parent = parent;
        return this;
    }

}
