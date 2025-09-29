using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PhotonArrow : MonoBehaviour//, IPunObservable
{
    //Vector3 nextPos;
    //Quaternion nextRot;
    //PhotonView pv;
    //float lifeTime = 3.0f;
    //bool isPosInit = false;
    //MeshRenderer[] mr;

    internal int    ownerId = -1; // -1이면 화살 주인 플레이어가 방 밖으로 나갔다는 의미
    internal string teamOfOwner = "blue"; // 어느 팀에서 쏜 것인지

    private void Awake()
    {
        //PhotonNetwork.SerializationRate = 30;

        //pv = GetComponent<PhotonView>();

        //if(!pv.IsMine)
        //{ 
        //    mr = GetComponentsInChildren<MeshRenderer>();
        //    for(int i =0; i < mr.Length; i++)
        //    {
        //        mr[i].enabled = false;
        //    }
        //}
    }
    private void Start()
    {
    }
    private void Update()
    {
        // 보간을 해주지 않으면 뚝뚝 끊겨보임
        //if(pv.IsMine == false)
        //{
        //    if(nextPos != null)
        //        transform.position = Vector3.Lerp(transform.position, nextPos, 10 * Time.deltaTime);
        //    if(nextRot != null)
        //        transform.rotation = Quaternion.Slerp(transform.rotation, nextRot, 10 * Time.deltaTime);
        //}

        //if(lifeTime > 0)
        //{
        //    lifeTime -= Time.deltaTime;
        //}
        
        //if(pv.IsMine && lifeTime <= 0)
        //    PhotonNetwork.Destroy(pv);
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        stream.SendNext(transform.position);
    //        stream.SendNext(transform.rotation);
    //    }
    //    else
    //    {
    //        nextPos = (Vector3)stream.ReceiveNext();
    //        nextRot = (Quaternion)stream.ReceiveNext();

    //        if(isPosInit == false)
    //        {
    //            isPosInit = true;
    //            transform.position = nextPos;
    //            if(!pv.IsMine)
    //            {
    //                for (int i = 0; i < mr.Length; i++)
    //                {
    //                    mr[i].enabled = true;
    //                }
    //            }
    //        }
    //    }
    //}
}
