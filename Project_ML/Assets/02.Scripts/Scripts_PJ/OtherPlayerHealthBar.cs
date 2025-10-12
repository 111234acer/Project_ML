using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class OtherPlayerHealthBar : MonoBehaviour
{
    public PlayerHealth_Copy playerHealth;
    public Slider healthSlider;
    public Transform target;      // 머리 기준점(없으면 playerHealth.transform)
    public Vector3 offset = new Vector3(0, 0, 0);

    public Canvas canvas;         // Screen Space - Overlay 캔버스
    public Camera cam;            // 메인 카메라

    public LayerMask occluderMask = ~0;
    public float losRadius = 0.12f;
    public float maxCheckDistance = 200f;

    public float hideBeyond = 50f;

    public float showAfterDamagedSeconds = 3f;

    RectTransform rect;
    RectTransform canvasRect;
    PhotonView ownerPv;
    CanvasGroup cg;

    static readonly Dictionary<int, float> s_DamagedUntil = new Dictionary<int, float>();

    void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);

        rect = transform as RectTransform;
        if (rect == null) { enabled = false; return; }
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        if (healthSlider == null) 
            healthSlider = GetComponentInChildren<Slider>(true);

        cg = GetComponentInChildren<CanvasGroup>(true);

        if (cg == null && healthSlider != null) 
            cg = healthSlider.gameObject.AddComponent<CanvasGroup>();

        if (cg != null) 
            cg.interactable = false; cg.blocksRaycasts = false;
    }
    void OnEnable()
    {
        if (healthSlider != null) 
            healthSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        if (cam == null)
            cam = (Camera.main != null) ? Camera.main : (Camera.allCamerasCount > 0 ? Camera.allCameras[0] : null);
        if (ownerPv == null && playerHealth != null)
            ownerPv = playerHealth.GetComponentInParent<PhotonView>();
        if (target == null && playerHealth != null)
            target = playerHealth.transform;
        if (canvasRect == null && canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        if (playerHealth == null || target == null || rect == null || healthSlider == null || cam == null || canvasRect == null)
            return;

        // 내 캐릭터 HP바는 숨김
        if (ownerPv != null && ownerPv.IsMine)
        {
            if (healthSlider.gameObject.activeSelf) healthSlider.gameObject.SetActive(false);
            return;
        }

        // 값 동기화
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.currentHealth;

        // 기준 월드 위치(머리) + 월드 오프셋
        Vector3 headWorld = target.position + offset;

        // 화면 좌표 변환 + 픽셀 보정(Y)
        Vector3 screenPos = cam.WorldToScreenPoint(headWorld);

        // 화면 내 체크
        bool inView = screenPos.z > 0 &&
                      screenPos.x >= 0 && screenPos.x <= Screen.width &&
                      screenPos.y >= 0 && screenPos.y <= Screen.height;

        float dist = Vector3.Distance(cam.transform.position, headWorld);
        if (!inView || dist > hideBeyond)
        {
            if (healthSlider.gameObject.activeSelf) healthSlider.gameObject.SetActive(false);
            return;
        }

        // 벽 가림
        Vector3 origin = cam.transform.position;
        Vector3 dir = headWorld - origin;
        float rayDist = Mathf.Min(dir.magnitude, maxCheckDistance);
        bool occluded = Physics.SphereCast(origin, losRadius, dir.normalized, out RaycastHit hit, rayDist, occluderMask, QueryTriggerInteraction.Ignore)
                        && !hit.transform.IsChildOf(target);

        if (occluded)
        {
            if (healthSlider.gameObject.activeSelf) healthSlider.gameObject.SetActive(false);
            return;
        }

        // ---- “맞았을 때만 표시” ----
        bool recentlyDamaged = false;
        if (ownerPv != null && s_DamagedUntil.TryGetValue(ownerPv.ViewID, out float until))
            recentlyDamaged = Time.time < until;

        if (!recentlyDamaged)
        {
            if (healthSlider.gameObject.activeSelf) healthSlider.gameObject.SetActive(false);
            return;
        }

        // Screen-Space 기준 배치(머리 기준 "딱 고정")
        Vector2 localPoint;
        Camera uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCam, out localPoint);
        rect.anchoredPosition = localPoint;

        // 크기/투명도 고정
        rect.localScale = Vector3.one;
        if (cg != null) cg.alpha = 1f;

        // 최종 표시
        if (!healthSlider.gameObject.activeSelf) healthSlider.gameObject.SetActive(true);
    }

    // 외부 알림: 내가 이 대상에게 피해 줬을 때 호출(필수)
    public static void RegisterDamagedByLocal(int targetViewId, float showSeconds = -1f)
    {
        if (showSeconds <= 0f) showSeconds = 3f;
        s_DamagedUntil[targetViewId] = Time.time + showSeconds;
    }
}