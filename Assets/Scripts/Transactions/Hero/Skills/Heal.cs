using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Heal : Action, IStateDependent, IBuff
{
    public BaseState state { get; protected set; }
    protected Dictionary<string, Func<object, object>> _selfStatChangeMapping;

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
        state.ApplyChanges(GetSelfModifiedStats(state));
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

        cooldown = 3;
        isAvailable = true;

        status = ActionStatus.HURT;
    }
    void Update() 
    {
        self_curHP_d = 50;
    }

    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        self_curHP_d = 50;

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

    public override void UseOnState(BaseState state, float cr)
    {
        return;
    }
}
