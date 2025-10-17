using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class ClientMotor : MonoBehaviourPun
{
    public float positionLerpSpeed = 10f;
    public float rotationLerpSpeed = 10f;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private void Start()
    {
        targetPos = transform.position;
        targetRot = transform.rotation;
    }

    private void Update()
    {
        if (!photonView.IsMine)
            SmoothMove();
    }

    void SmoothMove()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, positionLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * Time.deltaTime);
    }

    [PunRPC]
    public void Client_ApplySnapshot(Vector3 pos, Quaternion rot, float velY, bool grounded)
    {
        targetPos = pos;
        targetRot = rot;
    }
}
