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
    private readonly Dictionary<int, UnitController> byIndex = new Dictionary<int, UnitController>();  // 스폰인덱스 → 실체 (눈이 씀)
    private int nextSpawnIndex;   // 자체 발급 스폰 순번 — 타이브레이크의 근원 (GetInstanceID 금지)

    public override void Initialize()
    {
        pool = GameManager.ObjectPool.CreateObjectPool
        (
            unitPrefab,
            CreateUnit,
            OnGetFromPool,
            OnReleaseToPool
        );
    }

    public UnitController Spawn(string unitId, int team, Vector3 position)
    {
        UnitStats stats = UnitTableRepository.Get(unitId);
        var unit = pool.Get().GetComponent<UnitController>();
        unit.Setup(nextSpawnIndex++, team, stats, position);
        rosters[team].Add(unit);
        byIndex[unit.SpawnIndex] = unit;
        return unit;
    }

    public void Despawn(UnitController unit)
    {
        rosters[unit.Team].Remove(unit);
        byIndex.Remove(unit.SpawnIndex);
        pool.Release(unit.gameObject);
    }

    public IReadOnlyList<UnitController> Roster(int team)
    {
        return rosters[team];
    }

    // 스폰인덱스로 실체를 찾는다 — 없으면(이미 반환됐으면) null
    public UnitController Find(int spawnIndex)
    {
        UnitController unit;
        if (byIndex.TryGetValue(spawnIndex, out unit)) return unit;
        return null;
    }

    // 풀이 부르는 세 콜백 — 템플릿 CreateObjectPool이 함수를 인자로 받는다
    private GameObject CreateUnit()
    {
        return Instantiate(unitPrefab);
    }

    private void OnGetFromPool(GameObject unit)
    {
        unit.SetActive(true);
    }

    private void OnReleaseToPool(GameObject unit)
    {
        unit.SetActive(false);
    }

    public override void Clear()
    {
        rosters[0].Clear();
        rosters[1].Clear();
        byIndex.Clear();
        nextSpawnIndex = 0;
    }
}
