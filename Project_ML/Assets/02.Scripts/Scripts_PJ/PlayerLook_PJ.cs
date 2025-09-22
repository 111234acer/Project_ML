using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLook_PJ : MonoBehaviourPun
{
    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 200f;
    public float xRotationLimit = 80f;
    public float smoothSpeed = 10f;

    private float xRotation = 0f;

    void Update()
    {
        if (!photonView.IsMine) return; // 내 것만 조작
        Look();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
