using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Fireball : Action, IEffect, IProjectile, ITarget, ITransient, IReward
{
    public BaseAgent agent { get; protected set; }
    public float reward { get; protected set; }
    // ITransient
    public float duration { get; protected set; }
    // IStateDependent
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
    public virtual Stats GetModifiedStats(BaseState state)
    {
        // string[] statsArr = {"HP"};
        // List<string> statChange = new List<string>(statsArr);
        // Dictionary<PropertyInfo, object> stats = new Dictionary<PropertyInfo, object>();
        // foreach(string stat in statChange)
        // {
        //     PropertyInfo prop = state.GetType().GetProperty(stat);
        //     stats.Add(prop, _statChangeMapping[stat](prop.GetValue(state)));
            
        // }
        // return stats;
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
    // IProjectile
    public GameObject projectile { get; protected set; }
    // ITarget
    public BaseAnimResolver targetAnimResolver { get; protected set; }
    public ActionStatus targetStatus { get; protected set; }
    // Action
    public void OnHit(Collision2D other) 
    {
        if(other.gameObject.GetComponent<BaseState>() != null)
        {
            targetAnimResolver = other.gameObject.GetComponent<BaseAnimResolver>();
            targetAnimResolver.ChangeStatus(targetStatus);
            UseOnState(other.gameObject.GetComponent<BaseState>(), cr);
            agent.AddReward(reward);
        }
    }
    public override void Fire(float cr)
    {
        if(!isAvailable)
        {
            state.busy = false;
            return;
        }
        this.cr = cr;
        GameObject fireball = Instantiate(projectile, 
                                        new Vector2(transform.position.x, transform.position.y), 
                                        Quaternion.Euler(0, animResolver.faceTowards > 0 ? 0 : 180f, 0));
        FireballProjectile fball_proj = fireball.GetComponent<FireballProjectile>()
                                                .Initialize(Vector2.right * animResolver.faceTowards, 
                                                            Vector2.right * animResolver.faceTowards * 2000 * Time.fixedDeltaTime,
                                                            gameObject) as FireballProjectile;
        fball_proj.transform.parent = null;
        state.busy = false;
        StartCoroutine(StartCooldown(cr));
    }
    public override void UseOnState(BaseState state, float cr)
    {
        state.ApplyTimedChanges(GetModifiedStats(state), duration);
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
        status = ActionStatus.ATTACK;
        targetStatus = ActionStatus.HURT;

        cooldown = 7;
        isAvailable = true;
        
        projectile = Resources.Load<GameObject>("Prefabs/Projectiles/Fireball");
    }
    void Update() {
        curHP_d = -(int)(state.stats.AD * 0.2f) - 10;
        reward = -curHP_d / 100f;
    }
    public override Action Initialize(GameObject obj) 
    {
        animResolver = obj.GetComponent<BaseAnimResolver>();
        state = obj.GetComponent<BaseState>();
        agent = obj.GetComponent<BaseAgent>();
        curHP_d = -(int)(state.stats.AD * 0.2f) - 10;
        reward = -curHP_d / 100f;
        newStatus = Status.STUNNED;
        duration = 1;

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
        row[16] = (float)(int)newStatus;
        return row; 
    }
}

