using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DummySkillManager : BaseSkillManager
{
    void Awake()
    {
        actionController = GetComponent<HeroActionController>();
        currentSkills = new List<string>();
    }
    
}