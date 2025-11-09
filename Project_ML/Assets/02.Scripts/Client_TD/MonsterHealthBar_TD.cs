using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthBar_TD : MonoBehaviour
{    public enum FaceMode
    {
        CameraForward,   // 카메라의 forward 방향으로 평면 고정 (가장 일반적, 글씨가 항상 정면)
        CameraPosition,  // HP바가 카메라(시점) 쪽을 향하도록 회전(look-at)
        PlayerPosition   // HP바가 플레이어 월드 좌표를 바라보도록 회전(look-at)
    }

    public Monster_TD monster;
    public Transform anchor;
    public Camera targetCamera;
    public Transform player;

    public Canvas worldCanvas;
    public Slider hpSlider;

    public Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);
    public float worldScale = 0.0025f;
    public bool onlyRotateY = true;
    public FaceMode faceMode = FaceMode.CameraForward;

    void Reset()
    {
        monster = GetComponentInParent<Monster_TD>();
        anchor = transform;
    }

    void Awake()
    {
        if (!monster) monster = GetComponentInParent<Monster_TD>();
        if (!anchor) anchor = transform;

        if (!worldCanvas) worldCanvas = GetComponentInChildren<Canvas>(true);
        if (worldCanvas) worldCanvas.renderMode = RenderMode.WorldSpace;

        if (!hpSlider) hpSlider = GetComponentInChildren<Slider>(true);

        ApplyInitialUIState();
    }

    void OnEnable()
    {
        if (!targetCamera) targetCamera = Camera.main;
        UpdateHPImmediate();
    }
    void LateUpdate()
    {
        if (!monster || !worldCanvas) return;

        // 1) 위치
        var basePos = (anchor ? anchor.position : transform.position) + worldOffset;
        worldCanvas.transform.position = basePos;

        // 2) 회전(항상 평면 유지)
        EnsureTargetCamera();
        Vector3 forward;

        switch (faceMode)
        {
            case FaceMode.CameraPosition:
                // 카메라 위치를 ‘바라보는’ 방식
                if (targetCamera)
                    forward = (worldCanvas.transform.position - targetCamera.transform.position).normalized;
                else
                    forward = Vector3.forward;
                break;

            case FaceMode.PlayerPosition:
                // 플레이어 위치를 ‘바라보는’ 방식
                if (player)
                    forward = (worldCanvas.transform.position - player.position).normalized;
                else if (targetCamera)
                    forward = (worldCanvas.transform.position - targetCamera.transform.position).normalized;
                else
                    forward = Vector3.forward;
                break;

            default: // CameraForward
                // 카메라의 forward를 그대로 사용(전형적인 빌보드)
                forward = targetCamera ? targetCamera.transform.forward : Vector3.forward;
                break;
        }

        if (onlyRotateY)
        {
            forward = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (forward.sqrMagnitude < 1e-6f) forward = worldCanvas.transform.forward;
        }

        worldCanvas.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // 3) HP 반영
        if (hpSlider)
            hpSlider.value = monster.maxHP > 0f ? (monster.currentHP / Mathf.Max(0.0001f, monster.maxHP)) : 0f;
    }
    void ApplyInitialUIState()
    {
        if (worldCanvas)
            worldCanvas.transform.localScale = Vector3.one * worldScale;

        if (hpSlider)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.interactable = false;
            hpSlider.value = monster
                ? monster.currentHP / Mathf.Max(0.0001f, monster.maxHP)
                : 1f;
        }
    }

    public void UpdateHPImmediate()
    {
        if (hpSlider && monster)
            hpSlider.value = monster.maxHP > 0f ? (monster.currentHP / Mathf.Max(0.0001f, monster.maxHP)) : 0f;
    }

    void EnsureTargetCamera()
    {
        // 카메라가 비었거나 비활성화 상태면 재획득
        if (!targetCamera || !targetCamera.isActiveAndEnabled)
            targetCamera = Camera.main;
    }
}