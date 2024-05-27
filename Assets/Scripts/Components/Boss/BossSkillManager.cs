using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BossSkillManager : BaseSkillManager
{
    void Awake()
    {
        actionController = GetComponent<HeroActionController>();
        currentSkills = new List<string>();

        foreach(KeyValuePair<string, Type> kvp in Mappings.SkillMap)
            AddSkill(kvp.Key);
    }
    
}