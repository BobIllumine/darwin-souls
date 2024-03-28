using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAnimResolver : MonoBehaviour
{
    public ActionStatus status { get; protected set; }
    public int faceTowards { get; protected set; }
    public abstract void AnimateTrigger(string param);
    public abstract void AnimateBool(string param, bool value);
    public virtual void ChangeStatus(ActionStatus newStatus)
    {
        status = newStatus;
    }
    public abstract void ChangeFloat(string param, float mult);
    public virtual void ChangeFacedDirection(int direction)
    {
        transform.Rotate(new Vector3(0, direction == faceTowards ? 0 : 180f, 0));
        faceTowards = direction;
    }
}
