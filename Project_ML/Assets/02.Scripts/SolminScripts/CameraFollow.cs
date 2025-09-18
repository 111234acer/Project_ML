using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;            // Player
    public Vector3 offset = new Vector3(0, 2, -3);
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.0f); // 플레이어 머리 방향 바라보기
    }
}
