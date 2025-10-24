using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    [Header("Meta Info")]
    public string characterId = "Lumia";      // 선택 ID (프리팹 이름)
    public string displayName = "루미아";

    [Header("References")]
    public Image imagePortrait;
    public TMP_Text textName;
    public Image selectedBorder;
    public Image selectedBadge;
    public Button clickArea;

    CharacterSelectManager manager;

    void Awake()
    {
        manager = FindObjectOfType<CharacterSelectManager>();

        if (textName) textName.text = displayName;

        if (clickArea)
            clickArea.onClick.AddListener(OnClickSelect);

        SetSelected(false);
    }

    public void OnClickSelect()
    {
        if (manager == null) return;
        manager.SelectCharacter(characterId);
    }

    public void SetSelected(bool on)
    {
        if (selectedBorder)
            selectedBorder.color = new Color(0.486f, 0.776f, 1f, on ? 1f : 0f); // #7BC6FF
        if (selectedBadge)
            selectedBadge.color = new Color(1f, 1f, 1f, on ? 1f : 0f);
    }
}
