using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterCatalog", menuName = "Game/CharacterCatalog")]
public class CharacterCatalogAsset : ScriptableObject
{
    public List<CharacterData> list;
    public CharacterData Get(int id) => (id >= 0 && id < list.Count) ? list[id] : null;
}

[System.Serializable]
public class CharacterData
{
    public string displayName;
    public Role role;                  // Dealer / Healer / Tanker
    public Sprite portrait;
    public List<Sprite> skillIcons = new();
    public string prefabName;          // 인게임에서 스폰할 프리팹 이름
}

public enum Role { Dealer, Healer, Tanker }