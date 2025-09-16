using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]                 // CharacterController 자동추가
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // 플레이어 이동속도

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        Move();
    }

    void Move()
    {
        float horzontal = Input.GetAxis("Horizontal");          // A,D
        float vertical = Input.GetAxis("Vertical");             // W,S

        // 플레이어 바라보는 방향 기준 이동 방향 계산
        Vector3 direction = transform.right * horzontal + transform.forward * vertical;

        // 이동 적용
        controller.Move(direction * moveSpeed * Time.deltaTime);
    }
}
