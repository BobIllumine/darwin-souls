using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class VampireSlash : Action, IEffect, ITarget, IBuff, IReward
{
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
    
    // ITarget
    public BaseAnimResolver targetAnimResolver { get; protected set; }
    public ActionStatus targetStatus { get; protected set; }

    // IBuff
    public int self_maxHP_d { get; protected set; }
    public float self_maxHP_mult { get; protected set; }
    public int self_curHP_d { get; protected set; }
    public float self_curHP_mult { get; protected set; }
    public int self_AD_d { get; protected set; }
    public float self_AD_mult { get; protected set; }
    public float self_MS_d { get; protected set; }
    public float self_MS_mult { get; protected set; }
    public float self_AS_d { get; protected set; }
    public float self_AS_mult { get; protected set; }
    public float self_CR_d { get; protected set; }
    public float self_CR_mult { get; protected set; }
    public Status? self_newStatus { get; protected set; }
    public virtual Stats GetSelfModifiedStats(BaseState state)
    {
        Stats newStats = new Stats(state.stats);

        newStats.MaxHP = (int)(self_maxHP_mult * newStats.MaxHP + self_maxHP_d);
        newStats.HP = (int)(self_curHP_mult * newStats.HP + self_curHP_d);
        newStats.AD = (int)(self_AD_mult * newStats.AD + self_AD_d);
        newStats.MS = (float)(self_MS_mult * newStats.MS + self_MS_d);
        newStats.AS = (float)(self_AS_mult * newStats.AS + self_AS_d);
        newStats.CR = (float)(self_CR_mult * newStats.CR + self_CR_d);
        newStats.status = self_newStatus is null ? newStats.status : (Status)self_newStatus;
        return newStats;
    }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable)
        {
            state.busy = false;
            return;
        }
        this.cr = cr;
        animResolver.ChangeStatus(status);
        state.busy = false;
        StartCoroutine(StartCooldown(cr));
    }
    public override void UseOnState(BaseState state, float cr)
    {
        state.ApplyChanges(GetModifiedStats(state));
        this.state.ApplyChanges(GetSelfModifiedStats(this.state));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.GetComponent<BaseState>() != null)
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
        newStatus = null;

        self_curHP_d = 0;
        self_curHP_mult = 1f;
        self_maxHP_d = 0;
        self_maxHP_mult = 1f;
        self_AD_d = 0;
        self_AD_mult = 1f;
        self_MS_d = 0f;
        self_MS_mult = 1f;
        self_AS_d = 0f;
        self_AS_mult = 1f;
        self_CR_d = 0f;
        self_CR_mult = 1f;
        self_newStatus = null;

        cooldown = 5;
        isAvailable = true;

        status = ActionStatus.ATTACK;
        targetStatus = ActionStatus.HURT;
    }
    void Update() 
    {
        curHP_d = (int)(-state.stats.AD * 1.2f);
        self_curHP_d = (int)(curHP_d * 0.8f);
        reward = (-curHP_d + self_curHP_d) / 100f;
    }
    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        agent = obj.GetComponent<BaseAgent>();
        curHP_d = (int)(-state.stats.AD * 1.2f);
        self_curHP_d = (int)(curHP_d * 0.8f);
        reward = (-curHP_d + self_curHP_d) / 100f;
        return this;
    }

    public override float[] Serialize()
    {
        float[] row = Mappings.DefaultSkillRow;
        row[1] = (isAvailable ? 1f : 0f);
        row[4] = curHP_d;
        row[5] = curHP_mult;
        row[6] = maxHP_d;
        row[7] = maxHP_mult;
        row[8] = AD_d;
        row[9] = AD_mult;
        row[10] = MS_d;
        row[11] = MS_mult;
        row[12] = AS_d;
        row[13] = AS_mult;
        row[14] = CR_d;
        row[15] = CR_mult;
        row[16] = newStatus is null ? 0f : (float)(int)newStatus;
        row[17] = self_curHP_d;
        row[18] = self_curHP_mult;
        row[19] = self_maxHP_d;
        row[20] = self_maxHP_mult;
        row[21] = self_AD_d;
        row[22] = self_AD_mult;
        row[23] = self_MS_d;
        row[24] = self_MS_mult;
        row[25] = self_AS_d;
        row[26] = self_AS_mult;
        row[27] = self_CR_d;
        row[28] = self_CR_mult;
        return row; 
    }
}
