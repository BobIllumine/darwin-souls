using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using System;

public class SkillSensor : ISensor, IDisposable
{
    public BaseSkillManager skillManager;
    public float[,] tableData;
    public string sensorName;
    private int maxCols;
    private int maxRows;
    private ObservationSpec observationSpec;

    public SkillSensor(GameObject agent, string name, int maxRows, int maxCols)
    {
        this.skillManager = agent.GetComponent<BaseSkillManager>();
        this.maxCols = maxCols;
        this.maxRows = maxRows;
        sensorName = name;
        tableData = ConstructTable(true);
        observationSpec = ObservationSpec.Visual(1, this.maxRows, this.maxCols);
    }

    public int[] GetObservationShape()
    {
        return new int[] { maxRows, maxCols };
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
        for (int i = 0; i < maxRows; i++)
        {
            for (int j = 0; j < maxCols; j++)
            {
                writer[index++] = tableData[i, j];
            }
        }
        return index;
    }

    internal float[,] ConstructTable(bool build_default = false)
    {
        float[,] newData = new float[maxRows, maxCols];
        int currRow = 0;
        foreach((string skill, int index) in Mappings.SkillIndex)
        {   
            Action act = null;
            float[] newRow;
            if(!build_default)
                act = skillManager is null ? null : skillManager.GetActionInstance(skill);
            
            newRow = (act is null ? Mappings.DefaultSkillRow : act.Serialize());

            newRow[0] = index;
            newRow[2] = (Mappings.SkillMap[skill].GetInterface("IMobility") is null ? 0f : 1f);
            newRow[3] = build_default ? -1f : skillManager.GetSkillIndex(skill);

            for(int i = 0; i < newRow.Length; ++i)
            newData[currRow, i] = newRow[i] / 65535;

            ++currRow;
        }

        return newData;
    }
    public byte[] GetCompressedObservation()
    {
        return null;
    }
    public void Update()
    {
        tableData = ConstructTable();
    }

    public void Reset() { 
        tableData = ConstructTable(true);
    }

    public SensorCompressionType GetCompressionType()
    {
        return SensorCompressionType.None;
    }

    public ObservationSpec GetObservationSpec()
    {
        return observationSpec;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }
    public string GetName()
    {
        return sensorName;
    }

    public void Dispose()
    {
        tableData = null;
        skillManager = null;
    }
}
