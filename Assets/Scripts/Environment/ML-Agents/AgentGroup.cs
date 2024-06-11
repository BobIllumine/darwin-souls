using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class AgentGroup : MonoBehaviour
{
    [SerializeField] private List<Agent> agents; // List to store the agents in the group
    private SimpleMultiAgentGroup agentGroup; // MultiAgentGroup instance

    void Start()
    {
        agentGroup = new SimpleMultiAgentGroup();
        // Add agents to the group
        foreach (var agent in agents)
        {
            agentGroup.RegisterAgent(agent);
        }
    }

    void OnAgentDeath()
    {
        agentGroup.EndGroupEpisode();
    }
    void OnMaxStep()
    {
        int maxHP = -1;
        Agent best = null;
        foreach(var agent in agents)
        {
            var hp = agent.gameObject.GetComponent<BaseState>().stats.HP; 
            if(hp > maxHP)
            {
                maxHP = hp;
                best = agent;
            }
        }
        best.AddReward(maxHP / 100f);
        foreach(var agent in agents)
        {
            if(agent != best)
            {
                var penalty = agent.gameObject.GetComponent<BaseState>().stats.HP;
                agent.AddReward(-penalty / 100f);
            }
        }
        agentGroup.GroupEpisodeInterrupted();
    }
    void Update()
    {}

    
}
