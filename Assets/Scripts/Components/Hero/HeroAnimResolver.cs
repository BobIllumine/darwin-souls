using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;
using UnityEngine.Animations;

public class HeroAnimResolver : BaseAnimResolver
{
    private Animator animator;
    void Awake()
    {
        status = ActionStatus.IDLE;
        animator = GetComponent<Animator>();
        faceTowards = 1;
    }
    
    public override void ChangeStatus(ActionStatus newStatus)
    {
        base.ChangeStatus(newStatus);
        try
        {
            AnimateBool(Mappings.Bools[status], true);
        } catch(KeyNotFoundException)
        {
            AnimateTrigger(Mappings.Triggers[status]);
        }
    }
    public override void AnimateTrigger(string param)
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

    public override void AnimateBool(string param, bool value)
    {
        foreach(AnimatorControllerParameter parameter in animator.parameters) 
        {
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

    public override void ChangeFloat(string animName, float mult)
    {
        try
        {
            animator.SetFloat(animName, mult);
        }
        catch 
        {
            Debug.Log("bad luck kiddo");
            return;
        }
    }
}
