using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]                 // CharacterController �ڵ��߰�
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                                // �÷��̾� �̵��ӵ�

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

        // �÷��̾� �ٶ󺸴� ���� ���� �̵� ���� ���
        Vector3 direction = transform.right * horzontal + transform.forward * vertical;

        // �̵� ����
        controller.Move(direction * moveSpeed * Time.deltaTime);
    }
}
