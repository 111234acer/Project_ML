using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerCamera;                              // 카메라 Transform
    public float mouseSensitivity;                       // 마우스 감도
    public float xRotationAngle;                          // 카메라 상하 회전 최대 각도
    [SerializeField] private float smoothTime = 0.05f;          // 카메라 회전이 부드럽게 적용되는 회전 시간(작을수록 즉시 반응 클수록 느리고 부드럽게)

    private float xRotation = 0f;                               // 카메라 상하 회전 값
    // 부드러운 마우스 회전용
    private Vector2 currentMouseDelta;                          // 현재 적용되고 있는 마우스 이동값
    private Vector2 currentMouseDeltaVelocity;                  // 내부에서 사용하는 속도 값 SmoothDamp 게산용


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;               // 마우스 커서 고정(게임 플레이중 마우스 화면 밖으로 못나가게 고정
    }

    // Update is called once per frame
    void Update()
    {
        Look();
    }

    void Look()
    {
        // 마우스 입력 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime; // 좌우 
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // 상하 

        // SmoothDamp 이용하여 마우스 입력 부드럽게 변환
        Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
        currentMouseDelta.x = Mathf.SmoothDamp(currentMouseDelta.x, targetMouseDelta.x, ref currentMouseDeltaVelocity.x, smoothTime);
        currentMouseDelta.y = Mathf.SmoothDamp(currentMouseDelta.y, targetMouseDelta.y, ref currentMouseDeltaVelocity.y, smoothTime);

        // 마우스로 좌우 -> 캐릭터 회전
        transform.Rotate(Vector3.up * currentMouseDelta.x);

        // 상하 회전 -> 카메라가 움직인다
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -xRotationAngle, xRotationAngle);         // 위아래 회전 제한 (시야 제한)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
