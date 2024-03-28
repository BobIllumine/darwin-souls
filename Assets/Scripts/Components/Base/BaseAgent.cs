using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class BaseAgent : Agent
{
    protected BaseMovementController movementController;
    protected BaseActionController actionController;
    [SerializeField] protected GameObject opponent;

    public virtual void ResetParameters() 
    {
        
        // opponent = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Ronin"), new Vector3(-10, 2.5f, 0), Quaternion.identity);
    }
}
