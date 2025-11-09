using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCatalog
{
    private static CharacterCatalog _instance;
    public static CharacterCatalog Instance => _instance ??= new CharacterCatalog();

    private CharacterCatalogAsset _asset;
    private const string RES_PATH = "CharacterCatalog";

    private CharacterCatalog()
    {
        _asset = Resources.Load<CharacterCatalogAsset>(RES_PATH);
    }

    public CharacterData Get(int id) => _asset != null ? _asset.Get(id) : null;
}