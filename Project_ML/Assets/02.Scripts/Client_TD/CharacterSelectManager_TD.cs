using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectManager_TD : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSlot
    {
        public string CharacterName;
        public string displayName;

        public Button selectButton;
        public Image portraitImage;

        public GameObject playerPrefab;
    }

    public CharacterSlot[] characters;
    public TextMeshProUGUI currentNameLabel;
    public Image currentPortraitImage;

    public string ingameSceneTD = "TowerDefence";

    public static string SelectedCharacterName;
    public static GameObject SelectedCharacterPrefab;

    private int _currentIndex = -1;

    private void Start()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            int idx = i;
            if (characters[i].selectButton != null)
            {
                characters[i].selectButton.onClick.AddListener(() => OnClickCharacter(idx));
            }
        }
    }

    private void OnClickCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
            return;

        _currentIndex = index;
        CharacterSlot slot = characters[index];

        currentNameLabel.text = slot.displayName;

        currentPortraitImage.sprite = slot.portraitImage.sprite;

        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].selectButton.interactable = (i != index);
        }
    }

    public void OnClickDone()
    {
        if (_currentIndex < 0 || _currentIndex >= characters.Length) return;

        CharacterSlot slot = characters[_currentIndex];

        SelectedCharacterName = slot.CharacterName;
        SelectedCharacterPrefab = slot.playerPrefab;

        if (string.IsNullOrEmpty(ingameSceneTD) == false)
        {
            SceneManager.LoadScene(ingameSceneTD);
        }
    }
}
