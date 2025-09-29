using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonLook : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerCamera;                              // 카메라 Transform
    public float mouseSensitivity = 200f;                              // 마우스 감도
    public float xRotationLimit = 80f;                                // 상하 회전 제한
    public float smoothSpeed = 10f;                                   // 회전 부드럽게 적용하는 속도

    private float xRotation = 0f;                               // 카메라 상하 회전 값
    PhotonView pv;
    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (!pv.IsMine)
            playerCamera.GetComponent<Camera>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(pv.IsMine)
            Look();
    }

    void Look()
    {
        // 마우스 입력 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // 좌우 
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // 상하 

        // 좌우 회전(캐릭터 바디 회전)
        Quaternion targetBodyRotation = Quaternion.Euler(0, mouseX, 0) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetBodyRotation, smoothSpeed);

        // 상하 회전 -> 카메라가 움직인다
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);         // 위아래 회전 제한 (시야 제한)

        Quaternion targetCameraRotation = Quaternion.Euler(xRotation, 0, 0);
        playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetCameraRotation, smoothSpeed * Time.deltaTime);
    }
}
