using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerLook_Net : MonoBehaviourPun
{
    [Header("Camera Settings")]
    public Transform playerCamera;                  // 카메라 Transform
    public float mouseSensitivity = 200f;           // 마우스 감도
    public float xRotationLimit = 80f;              // 상하 회전 제한
    public float smoothSpeed = 10f;                 // 회전 부드럽게 적용 속도

    private float xRotation = 0f;                   // 카메라 상하 회전 값
    private float yaw;                              // 누적 yaw 값
    private float sendTimer;                        // RPC 전송 주기 타이머
    private const float sendInterval = 0.02f;       // 50Hz로 서버 회전 전송 (필요시 조정 가능)

    void Start()
    {
        if (photonView.IsMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            yaw = transform.eulerAngles.y;  // 현재 회전값으로 초기화 
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
        // 마우스 입력 가져오기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- 회전 누적 (절대 yaw) ---
        yaw += mouseX;

        // 일정 주기로만 서버 전송
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval && Mathf.Abs(mouseX) > 0.001f)
        {
            sendTimer = 0f;
            // ServerMotor의 PhotonView를 직접 찾아서 RPC 호출
            var serverMotor = GetComponent<ServerMotor>();
            if (serverMotor != null)
                serverMotor.photonView.RPC("Server_ReceiveYaw", RpcTarget.MasterClient, yaw);
        }

        // X축 회전(상하 시점)은 로컬 카메라에만 적용
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -xRotationLimit, xRotationLimit);

        Quaternion targetRot = Quaternion.Euler(xRotation, 0f, 0f);
        playerCamera.localRotation = Quaternion.Slerp(
            playerCamera.localRotation,
            targetRot,
            smoothSpeed * Time.deltaTime
        );
    }
}
