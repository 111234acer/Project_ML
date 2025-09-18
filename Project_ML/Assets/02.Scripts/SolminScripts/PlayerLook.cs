using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerCamera;                              // ī�޶� Transform
    public float mouseSensitivity;                              // ���콺 ����
    public float xRotationLimit;                                // ���� ȸ�� ����
    public float smoothSpeed;                                   // ȸ�� �ε巴�� �����ϴ� �ӵ�

    private float xRotation = 0f;                               // ī�޶� ���� ȸ�� ��

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Look();
    }

    void Look()
    {
        // ���콺 �Է� ��������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // �¿� 
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // ���� 
        
        // �¿� ȸ��(ĳ���� �ٵ� ȸ��)
        Quaternion targetBodyRotation = Quaternion.Euler(0,mouseX,0) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetBodyRotation, smoothSpeed);

        // ���� ȸ�� -> ī�޶� �����δ�
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation,-xRotationLimit, xRotationLimit);         // ���Ʒ� ȸ�� ���� (�þ� ����)

        Quaternion targetCameraRotation = Quaternion.Euler(xRotation, 0, 0);
        playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetCameraRotation, smoothSpeed * Time.deltaTime);
    }
}
