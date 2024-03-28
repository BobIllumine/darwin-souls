using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TerrainTools;

public class HeroState : BaseState
{
    [SerializeField] private int defaultMaxHP = 100;
    [SerializeField] private int defaultHP = 100;
    [SerializeField] private int defaultAD = 10;
    [SerializeField] private float defaultMS = 400.0f;
    [SerializeField] private float defaultAS = 1.0f;
    [SerializeField] private float defaultCR = 0.0f;
    [SerializeField] private Status defaultStatus = Status.OK;

    public override void ApplyChange(string name, object stat)
    {
        PropertyInfo info = this.GetType().GetProperty(name);
        ApplyChange((info, stat));
    }
    public override void ApplyChange((PropertyInfo, object) stat)
    {
        PropertyInfo prop = stat.Item1;
        object value = stat.Item2;
        lastImpact[prop] = (prop.GetValue(this), value);
        prop.SetValue(this, value);
    }

    public override void ApplyChanges(Dictionary<PropertyInfo, object> other)
    {
        foreach (KeyValuePair<PropertyInfo, object> pair in other)
            ApplyChange((pair.Key, pair.Value));
    }

    public override Dictionary<PropertyInfo, (object, object)> GetLastImpact()
    {
        return lastImpact;   
    }

    public override void ApplyTimedChanges(Dictionary<PropertyInfo, object> other, float duration)
    {
        Dictionary<PropertyInfo, object> copy = new Dictionary<PropertyInfo, object>();
        foreach (PropertyInfo prop in GetBaseProperties())
            copy.Add(prop, prop.GetValue(this));
        ApplyChanges(other);
        StartCoroutine(TimedRevert(copy, duration));
    }

    protected override void Update()
    {
        base.Update();
        animResolver.ChangeFloat("attackSpeed", AS);
    }

    public override void DestroyOnDeath()
    {
        base.DestroyOnDeath();
    }

    void Awake()
    {
        lastImpact = new Dictionary<PropertyInfo, (object, object)>();
        movementController = GetComponent<HeroMovementController>();
        actionController = GetComponent<HeroActionController>();
        animResolver = GetComponent<HeroAnimResolver>();
        MaxHP = defaultMaxHP;
        HP = defaultHP;
        AD = defaultAD;
        MS = defaultMS;
        AS = defaultAS;
        CR = defaultCR;
        status = defaultStatus;
    }
}
