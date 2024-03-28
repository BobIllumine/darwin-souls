using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameObject[] agents;
    private Dictionary<string, Vector3> positions;
    private Dictionary<string, BaseState> states;
    void Start()
    {
        agents = GameObject.FindGameObjectsWithTag("Agent");
        positions = new Dictionary<string, Vector3>();
        states = new Dictionary<string, BaseState>();
        foreach(var agent in agents) 
        {
            // print(agent);
            // positions[agent.name] = agent.transform.position;
            // states[agent.name] = agent.GetComponent<BaseState>();
        }

    }
    public Vector3 GetInitialPosition(string name) 
    {
        return positions[name];
    }

    public void ClearAll() 
    {
        foreach(var agent in agents) 
            Destroy(agent);
    }
}