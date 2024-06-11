using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HeroSkillManager : BaseSkillManager
{
    void Awake()
    {
        currentSkills = new List<string>();
    }
    void Start()
    {
        actionController = GetComponent<HeroActionController>();
    }
    
}