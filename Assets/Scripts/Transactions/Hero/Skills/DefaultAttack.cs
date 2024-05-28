using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DefaultAttack : Action, IEffect, ITarget, IMobility, IReward
{
    // IReward
    public BaseAgent agent { get; protected set; }
    public float reward { get; protected set; }
    // IEffect
    public int maxHP_d { get; protected set; }
    public float maxHP_mult { get; protected set; }
    public int curHP_d { get; protected set; }
    public float curHP_mult { get; protected set; }
    public int AD_d { get; protected set; }
    public float AD_mult { get; protected set; }
    public float MS_d { get; protected set; }
    public float MS_mult { get; protected set; }
    public float AS_d { get; protected set; }
    public float AS_mult { get; protected set; }
    public float CR_d { get; protected set; }
    public float CR_mult { get; protected set; }
    public Status? newStatus { get; protected set; }
    public Stats GetModifiedStats(BaseState state)
    {
        Stats newStats = new Stats(state.stats);

        newStats.MaxHP = (int)(maxHP_mult * newStats.MaxHP + maxHP_d);
        newStats.HP = (int)(curHP_mult * newStats.HP + curHP_d);
        newStats.AD = (int)(AD_mult * newStats.AD + AD_d);
        newStats.MS = (float)(MS_mult * newStats.MS + MS_d);
        newStats.AS = (float)(AS_mult * newStats.AS + AS_d);
        newStats.CR = (float)(CR_mult * newStats.CR + CR_d);
        newStats.status = newStatus is null ? newStats.status : (Status)newStatus;
        return newStats;
    }
    // IMobility
    public BaseMovementController movementController { get; protected set; }
    // ITarget
    public BaseAnimResolver targetAnimResolver { get; protected set; }
    public ActionStatus targetStatus { get; protected set; }
    // Action
    public override void Fire(float cr)
    {
        this.cr = cr;
        if (movementController.isGrounded)
            movementController.Stop();
        animResolver.ChangeStatus(status);
    }
    public override void UseOnState(BaseState state, float cr)
    {
        state.ApplyChanges(GetModifiedStats(state));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Agent") && other.gameObject.layer == 6 && other.gameObject.name != transform.parent.name)
        {
            targetAnimResolver = other.gameObject.GetComponent<BaseAnimResolver>();
            targetAnimResolver.ChangeStatus(targetStatus);
            UseOnState(other.gameObject.GetComponent<BaseState>(), cr);
            agent.AddReward(reward);
        }
    }
    void Awake() 
    {
        curHP_d = 0;
        curHP_mult = 1f;
        maxHP_d = 0;
        maxHP_mult = 1f;
        AD_d = 0;
        AD_mult = 1f;
        MS_d = 0f;
        MS_mult = 1f;
        AS_d = 0f;
        AS_mult = 1f;
        CR_d = 0f;
        CR_mult = 1f;
        reward = 0f;
        newStatus = null;
        status = ActionStatus.ATTACK;
        targetStatus = ActionStatus.HURT;
    }
    void Update() 
    {
        curHP_d = -state.stats.AD;
        reward = -curHP_d;
    }
    public override Action Initialize(GameObject obj) 
    {
        movementController = obj.GetComponent<BaseMovementController>();
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        agent = obj.GetComponent<BaseAgent>();
        curHP_d = -state.stats.AD;
        reward = -curHP_d;
        return this;
    }

    public override float[] Serialize()
    {
        throw new NotImplementedException();
    }
}
