using System.Collections.Generic;
using Game.Core.Data;
using UnityEngine;
using UnityEngine.Pool;

// 유닛의 수명 관리자 — 스폰 순번 발급·풀 대여/반환·팀별 로스터. 이 세 가지가 전부다.
// 전투 진행은 BattleManager, 국면은 MatchManager 몫 — 여기는 "누가 살아 있는가"의 명부만 안다.
//
// 09-08: 유닛별 프리팹이 생기면서 풀을 프리팹마다 하나씩 두게 바꿨다(그전엔 캡슐 한 종류라 풀도 하나).
// 프리팹은 unitId로 경로를 조립해 찾는다 — 유닛 8종이 역할 8개와 1:1이라 공유할 프리팹이 없고,
// 그래서 테이블에 경로 컬럼을 늘리지 않았다.
public class UnitManager : InGameManager
{
    private const string UnitPrefabFolder = "Prefabs/Units/";

    [SerializeField] private GameObject defaultUnitPrefab;   // 전용 프리팹이 아직 없는 종이 쓰는 몸
    [SerializeField] private Material allyMaterial;    // 팀 0 = 플레이어가 배치한 쪽
    [SerializeField] private Material enemyMaterial;   // 팀 1 = 적

    private readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();          // unitId → 프리팹
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();
    private readonly List<UnitController>[] rosters = { new List<UnitController>(), new List<UnitController>() };
    private readonly Dictionary<int, UnitController> byIndex = new Dictionary<int, UnitController>();  // 스폰인덱스 → 실체 (눈이 씀)
    private int nextSpawnIndex;   // 자체 발급 스폰 순번 — 타이브레이크의 근원 (GetInstanceID 금지)

    public UnitController Spawn(string unitId, int team, Vector3 position)
    {
        UnitStats stats = UnitTableRepository.Get(unitId);
        GameObject prefab = PrefabFor(unitId);
        GameObject body = PoolFor(prefab).Get();

        UnitController unit = body.GetComponent<UnitController>();
        unit.Setup(nextSpawnIndex++, team, stats, position);
        ApplyTeamColor(body, team);

        rosters[team].Add(unit);
        byIndex[unit.SpawnIndex] = unit;
        return unit;
    }

    public void Despawn(UnitController unit)
    {
        rosters[unit.Team].Remove(unit);
        byIndex.Remove(unit.SpawnIndex);

        GameObject prefab = PrefabFor(unit.Stats.UnitId);   // 나온 풀로 돌려보낸다 (캐시 조회라 싸다)
        PoolFor(prefab).Release(unit.gameObject);
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

    // ── 프리팹과 풀 ─────────────────────────────────────────────
    private GameObject PrefabFor(string unitId)
    {
        GameObject prefab;
        if (prefabCache.TryGetValue(unitId, out prefab)) return prefab;

        prefab = Resources.Load<GameObject>(UnitPrefabFolder + unitId);
        if (prefab == null)
        {
            prefab = defaultUnitPrefab;   // 아직 에셋이 없는 종도 전투는 돌아야 한다
        }

        prefabCache.Add(unitId, prefab);
        return prefab;
    }

    private ObjectPool<GameObject> PoolFor(GameObject prefab)
    {
        ObjectPool<GameObject> pool;
        if (pools.TryGetValue(prefab, out pool)) return pool;

        // 프리팹마다 생성 함수가 달라야 해서 이 자리에만 람다를 쓴다 (템플릿 API가 Func를 요구한다)
        pool = GameManager.ObjectPool.CreateObjectPool
        (
            prefab,
            () => Instantiate(prefab),
            OnGetFromPool,
            OnReleaseToPool
        );
        pools.Add(prefab, pool);
        return pool;
    }

    // 팀 색은 겉모습 담당이 입힌다 — 컨트롤러는 전투만 안다
    private void ApplyTeamColor(GameObject body, int team)
    {
        UnitView view = body.GetComponent<UnitView>();
        if (view == null) return;

        view.ApplyTeam(team == 0 ? allyMaterial : enemyMaterial);
    }

    // 풀이 부르는 두 콜백 — 템플릿 CreateObjectPool이 함수를 인자로 받는다
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
