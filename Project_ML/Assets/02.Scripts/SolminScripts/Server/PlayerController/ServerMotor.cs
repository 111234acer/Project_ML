using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class ServerMotor : MonoBehaviourPunCallbacks
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -20f;
    public float jumpHeight = 2f;
    public float fallMultiplier = 2.5f;
    public float jumpBufferTime = 0.1f;

    [Header("Ground Settings")]
    public LayerMask groundMask;

    [Header("Snapshot Settings")]
    [Tooltip("Server broadcast interval")]
    public float snapshotInterval = 1f / 30f;

    private CharacterController controller;
    private PlayerHealth_Server health;
    private AnimationHandler animationHandler;

    // internal state
    private float lastH, lastV;
    private bool requestJump;
    private float jumpBufferCounter;
    private float velocityY;
    private bool isGrounded;
    private bool prevGrounded;
    private float snapshotTimer;


    // world-space move vector (camera-based direction)
    private Vector3 lastMoveWorld = Vector3.zero;

    private bool ServerActive => PhotonNetwork.IsMasterClient;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<PlayerHealth_Server>();
        animationHandler = GetComponentInChildren<AnimationHandler>();
    }

    private void OnEnable()
    {
        if (!ServerActive)
        {
            //enabled = false;
            return;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //enabled = true;
            snapshotTimer = 0f;
        }
        else
        {
            //enabled = false;
        }
    }

    private void Update()
    {
        if (!ServerActive) return;
        if (health != null && health.isDead) return;

        if (requestJump)
        {
            jumpBufferCounter = jumpBufferTime;
            requestJump = false;
        }
        else
        {
            jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!ServerActive) return;
        if (health != null && health.isDead) return;

        GroundCheck();

        // move
        if (lastMoveWorld.sqrMagnitude > 1f) lastMoveWorld.Normalize();
        controller.Move(lastMoveWorld * moveSpeed * Time.fixedDeltaTime);

        // rotate smoothly toward move direction
        if (lastMoveWorld.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(lastMoveWorld.x, 0f, lastMoveWorld.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
        }

        // jump
        if (isGrounded && jumpBufferCounter > 0f)
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            photonView.RPC("Client_Anim_Jump", RpcTarget.All);
            jumpBufferCounter = 0f;
        }

        // gravity
        if (velocityY < 0f)
            velocityY += gravity * fallMultiplier * Time.fixedDeltaTime;
        else
            velocityY += gravity * Time.fixedDeltaTime;

        controller.Move(Vector3.up * velocityY * Time.fixedDeltaTime);

        // snapshot broadcast
        snapshotTimer += Time.fixedDeltaTime;
        if (snapshotTimer >= snapshotInterval)
        {
            Debug.Log($"[ServerMotor] Sending snapshot for ViewID {photonView.ViewID}");
            snapshotTimer = 0f;
            photonView.RPC("Client_ApplySnapshot", RpcTarget.All,
                transform.position, transform.rotation, velocityY, isGrounded);
        }
    }

    private void GroundCheck()
    {
        Vector3 center = controller.bounds.center;
        Vector3 spherePos = new Vector3(center.x, controller.bounds.min.y + 0.05f, center.z);
        float checkRadius = Mathf.Max(controller.radius * 0.9f, 0.2f);

        isGrounded = Physics.CheckSphere(spherePos, checkRadius, groundMask);
        if (isGrounded && velocityY < 0f)
            velocityY = -2f;

        if (!prevGrounded && isGrounded)
            photonView.RPC("Client_Anim_Land", RpcTarget.All);
        if (prevGrounded && !isGrounded)
            photonView.RPC("Client_Anim_Fall", RpcTarget.All);

        prevGrounded = isGrounded;
    }

    // ===== INPUT FROM CLIENT =====
    [PunRPC]
    public void Server_ReceiveInput(int viewID, float h, float v, bool jump, Vector3 forwardDir, PhotonMessageInfo info)
    {
        if (!ServerActive) return;
        if (photonView.ViewID != viewID) return;

        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);
        lastH = h;
        lastV = v;
        if (jump) requestJump = true;

        // world-space move direction from client camera
        Vector3 camFwd = forwardDir;
        camFwd.y = 0f;
        if (camFwd.sqrMagnitude < 0.001f) camFwd = transform.forward;
        camFwd.Normalize();
        Vector3 camRight = new Vector3(camFwd.z, 0f, -camFwd.x);

        lastMoveWorld = camRight * h + camFwd * v;

        photonView.RPC("Client_Anim_Move", RpcTarget.All, lastH, lastV);

    }

    [PunRPC] void Client_Anim_Move(float h, float v) => animationHandler?.OnMovement(h, v);
    [PunRPC] void Client_Anim_Jump() => animationHandler?.JumpTrigger();
    [PunRPC] void Client_Anim_Land() => animationHandler?.LandTrigger();
    [PunRPC] void Client_Anim_Fall() => animationHandler?.OnFall();
}
