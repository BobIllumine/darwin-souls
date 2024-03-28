using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BossAnimResolver : BaseAnimResolver
{
    private Animator animator;
    void Start()
    {
        status = ActionStatus.IDLE;
        animator = GetComponent<Animator>();
        faceTowards = 1;
    }
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, faceTowards == 1 ? 0 : 180f, 0);
    }
    public override void AnimateTrigger(string param)
    {
        ChangeStatus(status);
        // animator.SetTrigger(Mappings.EnemyTriggers[status]);
    }

    public override void AnimateBool(string param, bool value)
    {
        ChangeStatus(status);
        // foreach(string anim in Mappings.EnemyBools.Values)
        //     animator.SetBool(anim, false);
        // animator.SetBool(Mappings.EnemyBools[status], value);
    }

    public override void ChangeFloat(string param, float mult)
    {
        try
        {
            animator.SetFloat(param, mult);
        }
        catch {
            Debug.Log("bad luck kiddo");
            return;
        }
    }
}
