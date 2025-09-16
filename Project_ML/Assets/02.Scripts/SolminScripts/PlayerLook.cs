using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerCamera;                              // ī�޶� Transform
    public float mouseSensitivity;                       // ���콺 ����
    public float xRotationAngle;                          // ī�޶� ���� ȸ�� �ִ� ����
    [SerializeField] private float smoothTime = 0.05f;          // ī�޶� ȸ���� �ε巴�� ����Ǵ� ȸ�� �ð�(�������� ��� ���� Ŭ���� ������ �ε巴��)

    private float xRotation = 0f;                               // ī�޶� ���� ȸ�� ��
    // �ε巯�� ���콺 ȸ����
    private Vector2 currentMouseDelta;                          // ���� ����ǰ� �ִ� ���콺 �̵���
    private Vector2 currentMouseDeltaVelocity;                  // ���ο��� ����ϴ� �ӵ� �� SmoothDamp �Ի��


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;               // ���콺 Ŀ�� ����(���� �÷����� ���콺 ȭ�� ������ �������� ����
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

        // SmoothDamp �̿��Ͽ� ���콺 �Է� �ε巴�� ��ȯ
        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
        currentMouseDelta.x = Mathf.SmoothDamp(currentMouseDelta.x, targetMouseDelta.x, ref currentMouseDeltaVelocity.x, smoothTime);
        currentMouseDelta.y = Mathf.SmoothDamp(currentMouseDelta.y, targetMouseDelta.y, ref currentMouseDeltaVelocity.y, smoothTime);

        // ���콺�� �¿� -> ĳ���� ȸ��
        transform.Rotate(Vector3.up * currentMouseDelta.x);

        // ���� ȸ�� -> ī�޶� �����δ�
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -xRotationAngle, xRotationAngle);         // ���Ʒ� ȸ�� ���� (�þ� ����)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
