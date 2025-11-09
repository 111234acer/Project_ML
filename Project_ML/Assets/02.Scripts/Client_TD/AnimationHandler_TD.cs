using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHandler_TD : MonoBehaviour
{
    Animator animator;

    private static readonly int hashH = Animator.StringToHash("horizontal");
    private static readonly int hashV = Animator.StringToHash("vertical");
    private static readonly int hashIsGrounded = Animator.StringToHash("isGrounded");
    private static readonly int hashJumpT = Animator.StringToHash("jumpTrigger");
    private static readonly int hashLandT = Animator.StringToHash("landTrigger");
    private static readonly int hashAttackT = Animator.StringToHash("attackTrigger");


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnMovement(float horizontal, float vertical)
    {
        animator.SetFloat(hashH, horizontal);
        animator.SetFloat(hashV, vertical);
    }

    public void SetGrounded(bool grounded)
    {
        animator.SetBool(hashIsGrounded, grounded);
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

    public void AttackTrigger()
    {
        animator.SetTrigger(hashAttackT);
    }
}
