using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLook_Copy : MonoBehaviour
{
    PlayerHealth_Copy health;
    PhotonView pv;

    [Header("Camera Settings")]
    public Transform playerCamera;                              // 카메라 Transform
    public float mouseSensitivity = 200f;                              // 마우스 감도
    public float xRotationLimit = 80f;                                // 상하 회전 제한

    float yaw;    // Y축(수평) - 바디에만
    float pitch;  // X축(수직) - 카메라에만

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (playerCamera == null) playerCamera = Camera.main?.transform;

        health = GetComponent<PlayerHealth_Copy>();
    }

    void Start()
    {
        if (PhotonNetwork.InRoom && pv && playerCamera)
        {
            // 원격일 때만 비활성
            if (!pv.IsMine)
            {
                var cam = playerCamera.GetComponent<Camera>();
                if (cam) cam.enabled = false;
                var au = playerCamera.GetComponent<AudioListener>();
                if (au) au.enabled = false;
            }
        }
        if (pv && pv.IsMine && playerCamera)
        {
            var cam = playerCamera.GetComponent<Camera>();
            if (cam) cam.tag = "MainCamera";
            // 시작 각도 동기화
            yaw = transform.rotation.eulerAngles.y;
            pitch = playerCamera.localRotation.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f; // -180~180 보정
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine) return; // 로컬만 입력
        if (health != null && health.isDead) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;        // 바디 회전
        pitch -= mouseY;        // 카메라 상하
        pitch = Mathf.Clamp(pitch, -xRotationLimit, xRotationLimit);

        // 보간 없이 바로 적용(둔감함 제거). 필요하면 여기서만 Slerp 추가.
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (playerCamera) playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
