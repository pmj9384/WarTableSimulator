using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkinUserData : ISaveLoad
{
    public DataSourceType SaveDataSouceType => DataSourceType.Local;

    private readonly List<string> ownedSkinIds = new();

    private string equippedSkinId;
    public string EquippedSkinId => equippedSkinId;

    public Sprite GetEquippedSprite() =>
        Resources.Load<Sprite>($"Sprites/Skins/{equippedSkinId}");

    public event Action<string> OnSkinEquipped;

    public SkinUserData()
    {
        SaveLoadSystem.Instance.RegisterOnSaveAction(this);
    }

    public bool IsOwned(string skinId) => ownedSkinIds.Contains(skinId);

    public void Equip(string skinId)
    {
        if (!IsOwned(skinId)) return;
        equippedSkinId = skinId;
        OnSkinEquipped?.Invoke(equippedSkinId);
        SaveLoadSystem.Instance.Save();   // 구매·정산과 같은 즉시 durable 정책 (TongTongDefence 검수 v6 역이식)
    }
    public bool Unlock(string skinId)
    {
        if (IsOwned(skinId) == false)
        {
            ownedSkinIds.Add(skinId);
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Save()
    {
        var saveData = SaveLoadSystem.Instance.CurrentSaveData.skinUserDataSave = new();
        saveData.ownedSkinIds = new(ownedSkinIds);
        saveData.equippedSkinId = equippedSkinId;
    }

    public void Load()
    {
        ownedSkinIds.Clear();
        var defaultSkin = DataTableManager.SkinDataTable.All
        .FirstOrDefault(s => s.IsDefault);
        if (defaultSkin != null)
        {
            ownedSkinIds.Add(defaultSkin.SkinId);
            equippedSkinId = defaultSkin.SkinId;
        }
        else
        {
            return;
        }
    }

    public void Load(SkinUserDataSave saveData)
    {
        if (saveData == null) { Load(); return; }

        ownedSkinIds.Clear();
        ownedSkinIds.AddRange(saveData.ownedSkinIds);
        equippedSkinId = saveData.equippedSkinId;
    }
}
