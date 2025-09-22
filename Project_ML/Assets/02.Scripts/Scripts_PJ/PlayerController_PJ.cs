using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class PlayerController_PJ : MonoBehaviourPun
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -20f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float fallMultiplier = 2.5f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Settings")]
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private float jumpBufferCounter;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // 다른 플레이어 카메라/입력 비활성화
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return; // 내 캐릭터만 입력 처리

        GroundCheck();
        HandleJumpInput();
        Move();
    }

    void GroundCheck()
    {
        Vector3 spherePos = new Vector3(controller.bounds.center.x,
                                        controller.bounds.min.y + 0.05f,
                                        controller.bounds.center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);
        isGrounded = Physics.CheckSphere(spherePos, checkRadius, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;            
        }
        else
        {
            jumpBufferCounter = Mathf.Max(jumpBufferCounter - Time.deltaTime, 0);
        }

        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0;
        }
    }

    void ApplyGravity()
    {
        if (velocity.y < 0)
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        ApplyGravity();
        controller.Move(velocity * Time.deltaTime);
    }
}
