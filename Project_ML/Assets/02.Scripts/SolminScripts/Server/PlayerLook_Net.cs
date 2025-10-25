using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerLook_Net : MonoBehaviourPun
{
    [Header("Camera Settings")]
    public Transform playerCamera;              // 실제 카메라 Transform
    public float mouseSensitivity = 200f;       // 마우스 감도
    public float xRotationLimit = 80f;          // 상하 회전 제한
    public float rotationSmooth = 10f;          // 상하 회전 부드럽게 적용 속도

    [Header("Headup Camera Follow")]
    [Tooltip("카메라가 머리 위치를 따라가는 부드러움 (값이 낮을수록 즉각, 높을수록 부드러움)")]
    public float followSmoothTime = 0.05f;      // SmoothDamp 시간
    [Tooltip("Headup이 없을 때 기본 머리 높이 (fallback)")]
    public float headHeightOffset = 1.6f;       // 머리 위치 오프셋 (Headup 없을 때)
    private Vector3 followVelocity;             // SmoothDamp 속도 캐시
    private Transform headTarget;               // Headup Transform 참조용

    private float xRotation = 0f;               // 카메라 pitch 회전 값
    private float yaw;                          // 누적 yaw 값 (서버 전송용)
    private float sendTimer;                    // RPC 전송 주기 타이머
    private const float sendInterval = 1f / 30f; // 30Hz로 서버 회전 전송

    void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            yaw = transform.eulerAngles.y;

            // [HEADUP CAMERA] 머리 기준 Transform 자동 탐색
            var head = transform.Find("Headup");
            headTarget = head != null ? head : null;
        }
        else
        {
            // 다른 플레이어 카메라는 꺼줌
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        HandleLookInput();
    }

    void LateUpdate()
    {
        if (!photonView.IsMine || playerCamera == null)
            return;

        // [HEADUP CAMERA] 카메라 위치를 머리 위치로 부드럽게 이동
        Vector3 targetPos = (headTarget != null)
            ? headTarget.position
            : transform.position + Vector3.up * headHeightOffset;

        playerCamera.position = Vector3.SmoothDamp(
            playerCamera.position,
            targetPos,
            ref followVelocity,
            followSmoothTime
        );
    }

    void HandleLookInput()
    {
        // 마우스 입력 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- 회전 누적 (절대 yaw) ---
        yaw += mouseX;

        // [NETWORK] 일정 주기로 서버에 회전 전송 (30Hz)
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            var serverMotor = GetComponent<ServerMotor>();
            if (serverMotor != null)
                serverMotor.photonView.RPC("Server_ReceiveYaw", RpcTarget.MasterClient, yaw);
        }

        // X축 회전(상하 시점)은 로컬 카메라에만 적용
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);

        // [HEADUP CAMERA] 카메라 상하 회전을 부드럽게 적용
        Quaternion targetRot = Quaternion.Euler(xRotation, 0f, 0f);
        playerCamera.localRotation = Quaternion.Slerp(
            playerCamera.localRotation,
            targetRot,
            rotationSmooth * Time.deltaTime
        );
    }
}
