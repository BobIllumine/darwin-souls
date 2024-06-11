using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;

public class SkillSensorComponent : SensorComponent, IDisposable
{
    [SerializeField] private GameObject agent;
    [SerializeField] private string sensorName;
    [SerializeField] private int maxRows = 5;
    [SerializeField] private int maxCols = 5;
    [SerializeField] private int observationStacks;
    private SkillSensor sensor;

    public GameObject Agent {
        get { return agent; }
        set { agent = value; UpdateSensor(); }
    }

    void Start() 
    {
        UpdateSensor();
    }
    public SkillSensorComponent()
    {
        sensorName = "SkillSensor";
    }

    public override ISensor[] CreateSensors()
    {
        Dispose();
        sensor = new SkillSensor(agent, sensorName, maxRows, maxCols);
        if(observationStacks != 1)
            return new ISensor[] { new StackingSensor(sensor, observationStacks) };
        return new ISensor[] { sensor };
    }
    internal void UpdateSensor()
    {
        if(sensor != null)
        {
            sensor.skillManager = agent.GetComponent<BaseSkillManager>();
        }
    }
    public void Update() {}

    public void Reset() {}

    public string GetName()
    {
        return sensorName;
    }
    
    public void Dispose()
    {
        if(!ReferenceEquals(sensor, null))
        {
            sensor.Dispose();
            sensor = null;
        }
    }
}
