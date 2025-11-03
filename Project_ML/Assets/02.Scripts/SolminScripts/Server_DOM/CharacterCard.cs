using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    public Button btn;    

    [Header("Meta")]
    public int charId;            // 0~8
    public bool isDummy;          // 더미면 비활성

    CharacterSelectManager owner;

    public void Init(CharacterSelectManager o, int id)
    {
        owner = o;
        if (id >= 0) charId = id;

        if (!btn) btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => owner.HoverSelect(charId));

        // DisabledColor로 잠금 표현
        btn.interactable = !isDummy;
    }


    public void SetTaken(bool taken)
    {
        if (btn) btn.interactable = !(taken || isDummy);
    }
}
