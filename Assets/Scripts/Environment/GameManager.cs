using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] agents;
    private Dictionary<string, Vector3> positions;
    private Dictionary<string, Stats> stats;
    void Awake()
    {
        agents = GameObject.FindGameObjectsWithTag("Agent");
        positions = new Dictionary<string, Vector3>();
        stats = new Dictionary<string, Stats>();
        foreach(var agent in agents) 
        {
            print(agent.name);
            print(agent.transform.position);
            positions[agent.name] = agent.transform.position;
            stats[agent.name] = agent.GetComponent<BaseState>().stats;
        }
    }
    public Vector3 GetInitialPosition(string name) 
    {
        return positions[name];
    }
    public Stats GetInitialStats(string name)
    {
        return stats[name];
    }
    public void ClearAll() 
    {
        foreach(var agent in agents) 
            Destroy(agent);
    }
}