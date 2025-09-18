using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]                 // CharacterController 자동추가
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // 플레이어 이동속도
    public float jumpHeight = 2f;                               // 점프 높이
    public float gravity = -9.81f;                              // 중력 값

    private CharacterController controller;
    private Vector3 velocity;                                   // 현재 속도 (점프/중력) 포함

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;               // 마우스 커서 고정(게임 플레이중 마우스 화면 밖으로 못나가게 고정
    }
    void Update()
    {
        Move();
    }

    void Move()
    {
        float horzontal = Input.GetAxis("Horizontal");          // A,D
        float vertical = Input.GetAxis("Vertical");             // W,S
        
        Vector3 move = transform.right * horzontal + transform.forward * vertical;
        // 이동 적용
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 땅에 닿아있으면 속도 초기화
        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -0.1f;
        }

        // 점프 입력
        if(Input.GetButton("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
