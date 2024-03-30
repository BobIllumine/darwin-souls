using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Rage : Action, IStateDependent, IBuff, ITransient
{
    public BaseState state { get; protected set; }
    protected Dictionary<string, Func<object, object>> _selfStatChangeMapping;

    // ITransient
    public float duration { get; protected set; }
    
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
        Stats newStats = state.stats;

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
            return;
        this.cr = cr;
        animResolver.ChangeStatus(status);
        state.ApplyTimedChanges(GetSelfModifiedStats(state), duration);
        StartCoroutine(StartCooldown(cr));
    }
    void Start() 
    {
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
        self_newStatus = Status.OK;

        cooldown = 15;
        duration = 5;
        isAvailable = true;

        status = ActionStatus.HURT;
    }
    void Update() 
    {
        self_curHP_mult = 0.8f;
        self_AD_mult = 1.2f;
        self_AS_mult = 1.5f;
        self_MS_mult = 1.5f;  
    }
    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        self_curHP_mult = 0.8f;
        self_AD_mult = 1.2f;
        self_AS_mult = 1.5f;
        self_MS_mult = 1.5f;

        return this;
    }

    public override void UseOnState(BaseState state, float cr)
    {
        return;
    }
}
