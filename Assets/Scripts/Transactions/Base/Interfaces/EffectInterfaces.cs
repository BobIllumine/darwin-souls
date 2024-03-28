using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public interface IEffect
{
    public int maxHP_d { get; }
    public float maxHP_mult { get; }
    public int curHP_d { get; }
    public float curHP_mult { get; }
    public int AD_d { get; }
    public float AD_mult { get; }
    public float MS_d { get; }
    public float MS_mult { get; }
    public float AS_d { get; }
    public float AS_mult { get; }
    public float CR_d { get; }
    public float CR_mult { get; }
    public Status newStatus { get; }
    public Dictionary<PropertyInfo, object> GetModifiedStats(BaseState state);
}

public interface ITransient
{
    public float duration { get; }
}

public interface IPeriodic 
{
    public float period { get; }
}

public interface IBuff
{
    public int self_maxHP_d { get; }
    public float self_maxHP_mult { get; }
    public int self_curHP_d { get; }
    public float self_curHP_mult { get; }
    public int self_AD_d { get; }
    public float self_AD_mult { get; }
    public float self_MS_d { get; }
    public float self_MS_mult { get; }
    public float self_AS_d { get; }
    public float self_AS_mult { get; }
    public float self_CR_d { get; }
    public float self_CR_mult { get; }
    public Status self_newStatus { get; }
    public Dictionary<PropertyInfo, object> GetSelfModifiedStats(BaseState state);
}