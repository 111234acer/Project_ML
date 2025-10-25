using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class ClientSender : MonoBehaviourPun
{
    public float sendInterval = 1f / 30f;
    private float timer;

    private void Update()
    {
        if (!photonView.IsMine) return;

        timer += Time.deltaTime;
        if (timer >= sendInterval)
        {
            timer = 0f;
            SendInput();
        }
    }

    void SendInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool jump = Input.GetButton("Jump");
        bool dash = Input.GetKey(KeyCode.LeftShift);

        photonView.RPC("Server_ReceiveInput", RpcTarget.MasterClient,
            photonView.ViewID, h, v, jump, dash, Time.time);

        //Debug.Log($"[ClentSender] SendInput h {h} v : {v}");    멀티플레이 테스트 오류 확인용
    }
}
