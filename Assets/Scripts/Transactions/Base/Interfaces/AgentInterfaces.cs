using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public interface IReward 
{
    public BaseAgent agent { get; }
    public float reward { get; }
}