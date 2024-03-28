using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class VampireSlash : Action, IEffect, ITarget, IStateDependent, IBuff
{
    public BaseState state { get; protected set; }
    protected Dictionary<string, Func<object, object>> _statChangeMapping;
    protected Dictionary<string, Func<object, object>> _selfStatChangeMapping;
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
    public Status newStatus { get; protected set; }
    public Dictionary<PropertyInfo, object> GetModifiedStats(BaseState state)
    {
        string[] statsArr = {"HP"};
        List<string> statChange = new List<string>(statsArr);
        Dictionary<PropertyInfo, object> stats = new Dictionary<PropertyInfo, object>();
        foreach(string stat in statChange)
        {
            PropertyInfo prop = state.GetType().GetProperty(stat);
            stats.Add(prop, _statChangeMapping[stat](prop.GetValue(state)));
            
        }
        return stats;
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
    public Status self_newStatus { get; protected set; }
    public Dictionary<PropertyInfo, object> GetSelfModifiedStats(BaseState state) {
        string[] statsArr = {"HP"};
        List<string> statChange = new List<string>(statsArr);
        Dictionary<PropertyInfo, object> stats = new Dictionary<PropertyInfo, object>();
        foreach(string stat in statChange)
        {
            PropertyInfo prop = state.GetType().GetProperty(stat);
            stats.Add(prop, _selfStatChangeMapping[stat](prop.GetValue(state)));
        }
        return stats;
    }
    // Action
    public override void Fire(float cr)
    {
        if(!isAvailable)
            return;
        this.cr = cr;
        animResolver.ChangeStatus(status);
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
        }
    }
    void Start() 
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
        newStatus = Status.OK;

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

        cooldown = 3;
        isAvailable = true;

        status = ActionStatus.ATTACK;
        targetStatus = ActionStatus.HURT;
    }
    void Update() 
    {
        curHP_d = -state.AD;
        self_curHP_d = state.AD;
    }
    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        curHP_d = -state.AD;
        self_curHP_d = state.AD;

        _statChangeMapping = new Dictionary<string, Func<object, object>>() {
            ["MaxHP"] = max_hp => (int)(maxHP_mult * (int)max_hp + maxHP_d),
            ["HP"] = hp => (int)(curHP_mult * (int)hp + curHP_d),
            ["AD"] = ad => (int)(AD_mult * (int)ad + AD_d),
            ["MS"] = ms => (float)(MS_mult * (float)ms + MS_d),
            ["AS"] = @as => (float)(AS_mult * (float)@as + AS_d),
            ["CR"] = cr => (float)(CR_mult * (float)cr + CR_d)
        };
        _selfStatChangeMapping = new Dictionary<string, Func<object, object>>() {
            ["MaxHP"] = max_hp => (int)(self_maxHP_mult * (int)max_hp + self_maxHP_d),
            ["HP"] = hp => (int)(self_curHP_mult * (int)hp + self_curHP_d),
            ["AD"] = ad => (int)(self_AD_mult * (int)ad + self_AD_d),
            ["MS"] = ms => (float)(self_MS_mult * (float)ms + self_MS_d),
            ["AS"] = @as => (float)(self_AS_mult * (float)@as + self_AS_d),
            ["CR"] = cr => (float)(self_CR_mult * (float)cr + self_CR_d)
        };
        return this;
    }
}
