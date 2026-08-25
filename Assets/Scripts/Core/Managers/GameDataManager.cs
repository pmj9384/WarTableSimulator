using UnityCommunity.UnitySingleton;
using UnityEngine;

public class GameDataManager : PersistentMonoSingleton<GameDataManager>
{
    public PlayerAccountData PlayerAccountData { get; private set; }
    public StaminaSystem StaminaSystem { get; private set; }
    public SkinUserData SkinUserData { get; private set; }

    // TODO: 게임 특화 시스템 추가
    // public MyGameSystem MySystem { get; private set; }

    public override void InitializeSingleton()
    {
        base.InitializeSingleton();
        SaveLoadSystem.Instance.Load();

        PlayerAccountData = new();
        PlayerAccountData.Load(SaveLoadSystem.Instance.CurrentSaveData.playerAccountDataSave);

        StaminaSystem = new();
        StaminaSystem.Load(SaveLoadSystem.Instance.CurrentSaveData.staminaSystemSave);

        SkinUserData = new();
        SkinUserData.Load(SaveLoadSystem.Instance.CurrentSaveData.skinUserDataSave);

        // TODO: 게임 특화 시스템 초기화
        // MySystem = new(); MySystem.Load(...);
    }

    // 스태미나 회복 코루틴의 러너 — StaminaSystem은 non-MonoBehaviour라 스스로 코루틴을 돌릴 수 없다
    public Coroutine StartStaminaRecovery()
    {
        return StartCoroutine(StaminaSystem.CoRecovery());
    }
}
