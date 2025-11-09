using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput_CP))]
public class PlayerController_CP : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public float gravity = -9.81f;

    private CharacterController cc;
    private PlayerInput_CP input;
    private Animator anim;
    private Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput_CP>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleMovement();
        HandleAnimation();
    }

    void HandleMovement()
    {
        // 카메라 기준
        Vector3 camF = Camera.main.transform.forward; camF.y = 0;
        Vector3 camR = Camera.main.transform.right; camR.y = 0;

        Vector3 moveDir = camF * input.move.y + camR * input.move.x;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            cc.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        // 점프/중력
        if (cc.isGrounded)
        {
            velocity.y = input.jump ? jumpForce : -1f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        cc.Move(velocity * Time.deltaTime);
    }

    void HandleAnimation()
    {
        if (!anim) return;
        anim.SetBool("isMove", input.move.magnitude > 0.1f);
        anim.SetBool("isGrounded", cc.isGrounded);
    }
}
