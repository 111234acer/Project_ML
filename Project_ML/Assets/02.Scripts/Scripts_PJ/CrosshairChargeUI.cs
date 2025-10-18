using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairChargeUI : MonoBehaviour
{
    public Image chargeCircle;    // 차징 원
    
    public float maxSize = 1550f;  // 처음 크기
    public float minSize = 110f;   // 풀차지 시 크기

    private void OnEnable()
    {
        LumiaAttack_Net.OnChargeUpdate += UpdateUI;
    }

    private void OnDisable()
    {
        LumiaAttack_Net.OnChargeUpdate -= UpdateUI;
    }

    private void UpdateUI(float percent, bool active)
    {
        if (chargeCircle == null) return;

        chargeCircle.gameObject.SetActive(active);

        if (active)
        {
            float size = Mathf.Lerp(maxSize, minSize, percent);
            chargeCircle.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }
}