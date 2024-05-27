using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


// [RequireComponent(typeof(BossActionController))]
// [RequireComponent(typeof(BossMovementController))]
public class BossState : BaseState
{
    [SerializeField] private int defaultMaxHP = 100;
    [SerializeField] private int defaultHP = 100;
    [SerializeField] private int defaultAD = 10;
    [SerializeField] private float defaultMS = 400.0f;
    [SerializeField] private float defaultAS = 1.0f;
    [SerializeField] private float defaultCR = 0.0f;
    [SerializeField] private Status defaultStatus = Status.OK;

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
        // print(stats.HP);
        base.Update();
        animResolver.ChangeFloat("attackSpeed", stats.AS);
    }

    public override void OnDeath()
    {
        base.OnDeath();
        if(stats.HP <= 0)
        {
            // Destroy(gameObject);
            agent.AddReward(stats.MaxHP * 3);
            agent.EndEpisode(); 
        }
    }
    void Start()
    {
        agent = GetComponent<BossAgent>();
;
    }

    void Awake()
    {
        movementController = GetComponent<BossMovementController>();
        actionController = GetComponent<BossActionController>();
        animResolver = GetComponent<BossAnimResolver>();
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
