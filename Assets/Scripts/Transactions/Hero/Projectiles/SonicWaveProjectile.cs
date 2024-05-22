using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class SonicWaveProjectile : BaseProjectile
{
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animResolver = GetComponent<ProjectileAnimResolver>();
        animResolver.ChangeStatus(ProjectileStatus.CAST);
    }
    void FixedUpdate() {
        body.velocity = velocity;    
        animResolver.ChangeStatus(ProjectileStatus.MOVE);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.layer == 6 && other.gameObject.name != transform.parent.gameObject.name) {
            animResolver.ChangeStatus(ProjectileStatus.HIT);
            SendMessageUpwards("OnHit", other);
            DestroyOnHit();
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
