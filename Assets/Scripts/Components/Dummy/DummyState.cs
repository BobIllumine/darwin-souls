using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


public class DummyState : BaseState
{
    [SerializeField] private int defaultMaxHP = 100;
    [SerializeField] private int defaultHP = 100;
    [SerializeField] private int defaultAD = 10;
    [SerializeField] private float defaultMS = 400.0f;
    [SerializeField] private float defaultAS = 1.0f;
    [SerializeField] private float defaultCR = 0.0f;
    [SerializeField] private Status defaultStatus = Status.OK;
    [SerializeField] private GameObject target;

    public override void ApplyPeriodicChanges(Func<BaseState, Stats> statChange, float duration, float period)
    {
        StartCoroutine(PeriodicApply(statChange, duration, period));
    }

    public override void ApplyChanges(Stats other)
    {
        lastImpact = (stats, other);
        stats = new Stats(other);
    }

    public override (Stats, Stats) GetLastImpact()
    {
        return lastImpact;   
    }

    public override void ApplyTimedChanges(Stats other, float duration)
    {
        Stats copy = stats;
        ApplyChanges(other);
        StartCoroutine(TimedRevert(copy, duration));
    }

    public override void Update()
    {
        base.Update();
        animResolver.ChangeFloat("attackSpeed", stats.AS);
        // print(stats.HP);
    }

    public override void OnDeath()
    {
        base.OnDeath();
        if(stats.HP <= 0)
        {
            // target.GetComponent<BaseAgent>().AddReward(stats.MaxHP * 3);
            // stats.HP = stats.MaxHP;
        }
    }
    void Start()
    {
        agent = null;
    }

    void Awake()
    {
        movementController = GetComponent<DummyMovementController>();
        actionController = GetComponent<DummyActionController>();
        animResolver = GetComponent<DummyAnimResolver>();
        stats.MaxHP = defaultMaxHP;
        stats.HP = defaultHP;
        stats.AD = defaultAD;
        stats.MS = defaultMS;
        stats.AS = defaultAS;
        stats.CR = defaultCR;
        stats.status = defaultStatus;
        lastImpact = (null, stats);
    }
}
