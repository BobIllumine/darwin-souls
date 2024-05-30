using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] agents;
    private Dictionary<string, Vector3> positions;
    private Dictionary<string, Stats> stats;
    private Dictionary<string, Quaternion> rotations;
    void Awake()
    {
        positions = new Dictionary<string, Vector3>();
        stats = new Dictionary<string, Stats>();
        rotations = new Dictionary<string, Quaternion>();
    }
    void Start()
    {
        agents = GameObject.FindGameObjectsWithTag("Agent");
        foreach(var agent in agents) 
        {
            // print(agent.transform.position);
            positions[agent.name] = agent.transform.position;
            rotations[agent.name] = agent.transform.rotation;
            stats[agent.name] = agent.GetComponent<BaseState>().stats;
        }
    }
    public Vector3 GetInitialPosition(string name) 
    {
        return positions[name];
    }
    public Vector3 GetInitialRotation(string name) 
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