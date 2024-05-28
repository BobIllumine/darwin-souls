using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseSkillManager : MonoBehaviour
{
    public BaseActionController actionController { get; protected set; }
    public List<string> currentSkills { get; protected set; }

    virtual public void AddRandomSkill() {

        System.Random random = new System.Random();

        var filteredSkills = Mappings.SkillMap.Where(kv => !currentSkills.Contains(kv.Key)).ToList();

        if(filteredSkills.Count == 0)
            return;
        
        int index = random.Next(filteredSkills.Count);
        (string skillName, Type skill) = filteredSkills[index];
        
        actionController.AddAction(skillName, (gameObject.AddComponent(skill) as Action).Initialize(gameObject));
        currentSkills.Add(skillName);
    }

    virtual public void AddSkill(string name) {
        actionController.AddAction(name, (gameObject.AddComponent(Mappings.SkillMap[name]) as Action).Initialize(gameObject));
        currentSkills.Add(name);
    }

    virtual public string GetSkill(int index) {
        if(index < 0 || index >= currentSkills.Count)
            return "null";
        return currentSkills[index];
    }
    
    virtual public int GetSkillIndex(string name) {
        return currentSkills.IndexOf(name);
    }

    virtual public Action GetActionInstance(string name)
    {
        Action act;
        actionController.actionSpace.TryGetValue(name, out act);
        return act;
    }
}