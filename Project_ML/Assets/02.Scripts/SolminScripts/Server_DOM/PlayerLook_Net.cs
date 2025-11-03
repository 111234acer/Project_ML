using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerLook_Net : MonoBehaviourPun
{
    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 200f;
    public float xRotationLimit = 80f;
    public float smoothSpeed = 10f;

    private float xRotation = 0f;
    private float yaw;

    void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            yaw = transform.eulerAngles.y;
        }
        else
        {
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        Look();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);

        // immediate local rotation (no network)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
