using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;

public class SkillSensor : ISensor
{
    private BaseSkillManager skillManager;
    private float[,] tableData;
    private string sensorName;
    private int maxCols;
    private int maxRows;
    private ObservationSpec observationSpec;

    public SkillSensor(BaseSkillManager skillManager, string name, int maxRows, int maxCols)
    {
        this.skillManager = skillManager;
        this.maxCols = maxCols;
        this.maxRows = maxRows;
        sensorName = name;
        tableData = ConstructTable();
        observationSpec = ObservationSpec.Vector(tableData.Length);
    }

    public int[] GetObservationShape()
    {
        return new int[] { tableData.GetLength(0), tableData.GetLength(1) };
    }

    public int Write(ObservationWriter writer)
    {
        int index = 0;
        for (int i = 0; i < tableData.GetLength(0); i++)
        {
            for (int j = 0; j < tableData.GetLength(1); j++)
            {
                writer[index++] = tableData[i, j];
            }
        }
        return index;
    }

    internal float[,] ConstructTable()
    {
        float[,] newData = new float[maxRows, maxCols];
        int currRow = 0;
        foreach((string skill, int index) in Mappings.SkillIndex)
        {
            Action act = skillManager.GetActionInstance(skill);
            float[] newRow;
            
            newRow = (act is null ? Mappings.DefaultSkillRow : act.Serialize());

            newRow[0] = index;
            newRow[2] = (Mappings.SkillMap[skill].GetInterface("IMobility") is null ? 0f : 1f);
            newRow[3] = 0f; // TODO: Do something with buttons please

            for(int i = 0; i < newRow.Length; ++i)
            newData[currRow, i] = newRow[i];

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

    public void Reset()
    {
        
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
}
