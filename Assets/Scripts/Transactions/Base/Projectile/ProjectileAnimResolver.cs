using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ProjectileAnimResolver : MonoBehaviour
{
    private Animator animator;
    public ProjectileStatus status { get; protected set; }
    public int faceTowards { get; protected set; }
    public void AnimateIdle(ProjectileStatus status) 
    {
        ChangeStatus(status);
    }
    public void AnimateTrigger(ProjectileStatus status)
    {
        ChangeStatus(status);
        animator.SetTrigger(Mappings.ProjectileTriggers[status]);
    }
    public void ChangeStatus(ProjectileStatus newStatus)
    {
        status = newStatus;
    }
    public void ChangeFacedDirection(int direction)
    {
        faceTowards = direction;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        status = ProjectileStatus.CAST;
    }
}

