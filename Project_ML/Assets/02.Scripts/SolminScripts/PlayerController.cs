using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // 이동속도

    [Header("Camera Settings")]
    public Transform playerCamera;                              // 카메라 Transform
    public float mouseSensitivity = 100f;                       // 마우스 감도
    public float xRotationAngle = 80f;

    private CharacterController controller;
    private float xRotation = 0f;                               // 카메라 상하 회전 값

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;               // 마우스 커서 고정(게임 플레이중 마우스 화면 밖으로 못나가게 고정
    }
    void Update()
    {
        Move();
        Look();
    }

    void Move()
    {
        float horzontal = Input.GetAxis("Horizontal");          // A,D
        float vertical = Input.GetAxis("Vertical");             // W,S

        // 플레이어 바라보는 방향 기준 이동
        Vector3 direction = transform.right * horzontal + transform.forward * vertical;
        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 마우스로 좌우 -> 캐릭터 회전
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전 -> 카메라가 움직인다
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationAngle, xRotationAngle);         // 위아래 각도 고개 제한
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
