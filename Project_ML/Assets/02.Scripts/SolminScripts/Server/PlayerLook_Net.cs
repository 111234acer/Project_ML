using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLook_Net : MonoBehaviourPun
{
    [Header("Camera Settings")]
    public Transform playerCamera;                  // 카메라 Transform
    public float mouseSensitivity = 200f;           // 마우스 감도
    public float xRotationLimit = 80f;              // 상하 회전 제한
    public float smoothSpeed = 10f;                 // 회전 부드럽게 적용 속도

    private float xRotation = 0f;                   // 카메라 상하 회전 값

    void Start()
    {
        //  내 캐릭터만 카메라 제어
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // 다른 캐릭터의 카메라는 비활성화 (본인만 시점 가짐)
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        //  내 캐릭터가 아니면 입력 무시
        if (!photonView.IsMine) return;

        Look();
    }

    void Look()
    {
        // 마우스 입력 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 좌우 회전 (플레이어 본체)
        Quaternion targetBodyRotation = Quaternion.Euler(0, mouseX, 0) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetBodyRotation, smoothSpeed * Time.deltaTime);

        // 상하 회전 (카메라)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);

        Quaternion targetCameraRotation = Quaternion.Euler(xRotation, 0, 0);
        playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetCameraRotation, smoothSpeed * Time.deltaTime);
    }
}
