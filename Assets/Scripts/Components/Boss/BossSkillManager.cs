using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossSkillManager : BaseSkillManager
{
    void Awake()
    {
        currentSkills = new List<string>();
    }
    void Start()
    {
        actionController = GetComponent<BossActionController>();
        foreach(KeyValuePair<string, Type> kvp in Mappings.SkillMap)
            AddSkill(kvp.Key);
    }
    
}