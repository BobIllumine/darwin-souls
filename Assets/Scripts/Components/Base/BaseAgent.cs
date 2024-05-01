using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public abstract class BaseAgent : Agent
{
    protected BaseMovementController movementController;
    protected BaseActionController actionController;

    protected BaseInput input;
    [SerializeField] protected GameObject opponent;
    [SerializeField] protected GameObject self;
    // [SerializeField] protected GameManager manager;
    protected Vector3 initialPos;
    protected Quaternion initialRot;
    protected Stats initialStats;

    public abstract void ResetParameters();
}
