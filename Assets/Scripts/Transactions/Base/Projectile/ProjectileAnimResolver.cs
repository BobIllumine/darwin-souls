using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ProjectileAnimResolver : MonoBehaviour
{
    private Animator animator;
    public ProjectileStatus status { get; protected set; }
    public int faceTowards { get; protected set; }
    public void ChangeStatus(ProjectileStatus status)
    {
        try
        {
            // if(newStatus == ActionStatus.DIE)
            //     print("try #1");
            AnimateBool(Mappings.ProjectileBools[status], true);
        } catch(KeyNotFoundException)
        {
            // if(newStatus == ActionStatus.DIE)
            //     print("try #2");
            AnimateTrigger(Mappings.ProjectileTriggers[status]);
        } finally {
            // if(newStatus == ActionStatus.IDLE)
                // print("wtf");
        }
    }
    public void AnimateTrigger(string param)
    {
        try 
        {
            animator.SetTrigger(param);
        }
        catch 
        {
            Debug.Log("bad luck kiddo");
            return;
        }
    }

    public void AnimateBool(string param, bool value)
    {
        foreach(AnimatorControllerParameter parameter in animator.parameters) 
        {
            // print(parameter.name);
            if(parameter.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(parameter.name, false);            
        }
        try 
        {
            animator.SetBool(param, true);
        }       
        catch 
        {
            Debug.Log("bad luck kiddo");
            return;
        }
    }
    public void ChangeFacedDirection(int direction)
    {
        // transform.Rotate(new Vector3(0, direction > 0 ? 0 : 180f, 0));
        faceTowards = direction;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        status = ProjectileStatus.CAST;
    }
}



