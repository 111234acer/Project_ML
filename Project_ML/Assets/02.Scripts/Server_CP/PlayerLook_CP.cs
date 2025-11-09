using UnityEngine;
using Photon.Pun;

public class PlayerLook_CP : MonoBehaviourPun
{
    [Header("Camera Settings")]
    public Transform playerCamera;            // 카메라 Transform
    public float mouseSensitivity = 200f;     // 마우스 감도
    public float xRotationLimit = 80f;        // 상하 회전 제한
    public float smoothSpeed = 10f;           // 회전 부드럽게 적용하는 속도

    private float xRotation = 0f;             // 카메라 상하 회전 값

    void Update()
    {
        if (!photonView.IsMine) return; // 내 캐릭터만 카메라 회전 허용
        Look();
    }

    void Look()
    {
        // 마우스 입력
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 좌우 회전 (플레이어 바디 회전)
        Quaternion targetBodyRotation = Quaternion.Euler(0, mouseX, 0) * transform.rotation;
        transform.rotation = targetBodyRotation;

        // 상하 회전 (카메라 회전)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);
        Quaternion targetCameraRotation = Quaternion.Euler(xRotation, 0, 0);
        playerCamera.localRotation = targetCameraRotation;
    }
}
