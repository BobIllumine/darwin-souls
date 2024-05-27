using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;

public class SkillSensorComponent : SensorComponent
{
    [SerializeField] private GameObject agent;
    [SerializeField] private string sensorName;
    [SerializeField] private int maxRows = 5;
    [SerializeField] private int maxCols = 5;
    [SerializeField] private int observationStacks;
    private SkillSensor sensor;
    private ObservationSpec observationSpec;
    private CompressionSpec compressionSpec;

    public SkillSensorComponent()
    {
        sensorName = "SkillSensor";
        observationSpec = ObservationSpec.Vector(maxRows * maxCols);
        compressionSpec = CompressionSpec.Default();
    }

    public override ISensor[] CreateSensors()
    {
        sensor = new SkillSensor(agent.GetComponent<BaseSkillManager>(), sensorName, maxRows, maxCols);
        if(observationStacks != 1)
            return new ISensor[] { new StackingSensor(sensor, observationStacks) };
        return new ISensor[] { sensor };
    }

    internal void UpdateSensor()
    {

    }

    public void Update()
    {
        // Optional: Update tableData here if needed
    }

    public void Reset()
    {
        // Optional: Reset tableData here if needed
    }

    public string GetName()
    {
        return sensorName;
    }
}
