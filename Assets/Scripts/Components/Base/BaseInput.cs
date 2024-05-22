using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseInput : MonoBehaviour
{
    [SerializeField] protected Player player;
    protected Dictionary<Button, KeyCode?> buttons;
    protected LimitedQueue<InputAction> queue;
    protected float buffer;
    protected List<string> skillList;
    protected string axis;
    public BaseActionController actionController { get; protected set; }
    public BaseMovementController movementController { get; protected set; }
    public abstract void BufferButton(Button button);
}
