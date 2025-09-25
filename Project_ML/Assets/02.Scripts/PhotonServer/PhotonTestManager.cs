using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonTestManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Points")]
    public Transform[] redSpawnPoints;
    public Transform[] blueSpawnPoints;


    private PhotonView pv = null;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        CreatePlayer();
    }

    private void Update()
    {

    }

    void CreatePlayer()
    {
        GameObject myObj = PhotonNetwork.Instantiate("PhotonPlayer", new Vector3(0, 3.8f, 50), Quaternion.identity, 0);
        Camera.main.transform.parent = myObj.transform;
        Camera.main.transform.position = myObj.transform.position + new Vector3(0, 2f, -2f);
        Debug.Log("»ý¼º");
    }
}