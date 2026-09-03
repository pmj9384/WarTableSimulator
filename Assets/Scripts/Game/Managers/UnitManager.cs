using System.Collections.Generic;
using Game.Core.Data;
using UnityEngine;
using UnityEngine.Pool;

// 유닛의 수명 관리자 — 스폰 순번 발급·풀 대여/반환·팀별 로스터. 이 세 가지가 전부다.
// 전투 진행은 BattleManager, 국면은 MatchManager 몫 — 여기는 "누가 살아 있는가"의 명부만 안다.
public class UnitManager : InGameManager
{
    [SerializeField] private GameObject unitPrefab;   // 임시 캡슐 — Task 6에서 Unit_Grunt 프리팹으로

    private ObjectPool<GameObject> pool;
    private readonly List<UnitController>[] rosters = { new List<UnitController>(), new List<UnitController>() };
    private int nextSpawnIndex;   // 자체 발급 스폰 순번 — 타이브레이크의 근원 (GetInstanceID 금지)

    public override void Initialize()
    {
        pool = GameManager.ObjectPool.CreateObjectPool(
            unitPrefab,
            () => Instantiate(unitPrefab),
            obj => obj.SetActive(true),
            obj => obj.SetActive(false));
    }

    public UnitController Spawn(string unitId, int team, Vector3 position)
    {
        UnitStats stats = UnitTableRepository.Get(unitId);
        var unit = pool.Get().GetComponent<UnitController>();
        unit.Setup(nextSpawnIndex++, team, stats, position);
        rosters[team].Add(unit);
        return unit;
    }

    public void Despawn(UnitController unit)
    {
        rosters[unit.Team].Remove(unit);
        pool.Release(unit.gameObject);
    }

    public IReadOnlyList<UnitController> Roster(int team) => rosters[team];

    public override void Clear()
    {
        rosters[0].Clear();
        rosters[1].Clear();
        nextSpawnIndex = 0;
    }
}
