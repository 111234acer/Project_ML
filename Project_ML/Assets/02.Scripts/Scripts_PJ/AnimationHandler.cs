using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    Animator animator;

    private static readonly int hashH = Animator.StringToHash("horizontal");
    private static readonly int hashV = Animator.StringToHash("vertical");
    private static readonly int hashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int hashDeadT = Animator.StringToHash("deadTrigger");    
    private static readonly int hashJumpT = Animator.StringToHash("jumpTrigger");
    private static readonly int hashLandT = Animator.StringToHash("landTrigger");
    private static readonly int hashIsAim = Animator.StringToHash("isAim");
    private static readonly int hashShootT = Animator.StringToHash("shootTrigger");
    private static readonly int hashSkill1T = Animator.StringToHash("skillTrigger1");
    private static readonly int hashSkill2T = Animator.StringToHash("skillTrigger2");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnMovement(float horizontal, float vertical)
    {
        animator.SetFloat(hashH, horizontal);
        animator.SetFloat(hashV, vertical);
    }

    public void JumpTrigger()
    {
        animator.ResetTrigger(hashLandT);
        animator.ResetTrigger(hashJumpT);
        animator.SetBool(hashIsGrounded, false);
        animator.SetTrigger(hashJumpT);
    } 

    public void OnFall()
    {
        animator.ResetTrigger(hashJumpT);
        animator.ResetTrigger(hashLandT);
        animator.SetBool(hashIsGrounded, false);
    }

    public void LandTrigger()
    {
        animator.speed = 1f;

        animator.ResetTrigger(hashJumpT);
        animator.SetBool(hashIsGrounded, true);
        animator.SetTrigger(hashLandT);
    }

    public void OnDead()
    {
        animator.SetTrigger(hashDeadT);
    }

    public void Respawn()
    {
        animator.SetTrigger(hashDeadT);
        animator.SetBool(hashIsGrounded, true);
        animator.SetFloat(hashH, 0f);
        animator.SetFloat(hashV, 0f);

        animator.Rebind();
    }

    public void OnAim()
    {
        animator.SetBool(hashIsAim, true);
    }

    public void ShootTrigger()
    {
        animator.SetTrigger(hashShootT);
        animator.SetBool(hashIsAim, false);
    }

    public void Skill1Trigger()
    {
        animator.SetTrigger(hashSkill1T);
    }

    public void Skill2Trigger()
    {
        animator.SetTrigger(hashSkill2T);
        
    }
}
