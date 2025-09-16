using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // �̵��ӵ�

    [Header("Camera Settings")]
    public Transform playerCamera;                              // ī�޶� Transform
    public float mouseSensitivity = 100f;                       // ���콺 ����
    public float xRotationAngle = 80f;

    private CharacterController controller;
    private float xRotation = 0f;                               // ī�޶� ���� ȸ�� ��

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;               // ���콺 Ŀ�� ����(���� �÷����� ���콺 ȭ�� ������ �������� ����
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

        // �÷��̾� �ٶ󺸴� ���� ���� �̵�
        Vector3 direction = transform.right * horzontal + transform.forward * vertical;
        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ���콺�� �¿� -> ĳ���� ȸ��
        transform.Rotate(Vector3.up * mouseX);

        // ���� ȸ�� -> ī�޶� �����δ�
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationAngle, xRotationAngle);         // ���Ʒ� ���� �� ����
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
