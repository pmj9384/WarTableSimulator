using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 유닛을 따라다니는 화면 표시 담당 — 지금은 체력바, 나중에 사망 이펙트·상태이상 아이콘도 여기.
// 화면에 고정된 표시(잔존 인구 게이지·지속 피해 테두리, 스펙 9절)는 여기 몫이 아니라 UI 계층 몫이다.
//
// 캔버스를 유닛마다 두지 않은 이유: 캔버스 하나가 리빌드 단위라 80기면 캔버스가 80개다.
// 화면 캔버스 하나에 모아 그리고, 위치는 매 프레임 월드→화면 좌표로 변환해 넣는다.
public class HudManager : InGameManager
{
    [SerializeField] private RectTransform hudRoot;      // 체력바가 담길 캔버스 안 빈 오브젝트
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float barHeightOffset = 2.2f;   // 유닛 머리 위 (월드 단위)

    private ObjectPool<GameObject> barPool;
    private readonly Dictionary<int, HealthBar> bars = new Dictionary<int, HealthBar>();   // 스폰인덱스 → 바
    private readonly List<int> orphanBuffer = new List<int>();   // 주인이 사라진 바 (순회 도중 제거 금지)

    public override void Initialize()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        if (healthBarPrefab == null)
        {
            Debug.LogWarning($"{name}: 체력바 프리팹이 비어 있어 표시를 건너뜁니다.");
            return;
        }

        barPool = GameManager.ObjectPool.CreateObjectPool
        (
            healthBarPrefab,
            CreateBar,
            OnGetFromPool,
            OnReleaseToPool
        );
    }

    // LateUpdate인 이유: 카메라가 움직인 뒤에 화면 좌표를 잡아야 바가 유닛에서 한 프레임 밀리지 않는다
    private void LateUpdate()
    {
        if (barPool == null) return;
        if (hudRoot == null) return;
        if (worldCamera == null) return;

        for (int team = 0; team < 2; team++)
        {
            UpdateTeamBars(team);
        }

        ReleaseOrphanBars();
    }

    private void UpdateTeamBars(int team)
    {
        IReadOnlyList<UnitController> roster = GameManager.Units.Roster(team);
        for (int i = 0; i < roster.Count; i++)
        {
            UnitController unit = roster[i];
            if (NeedsBar(unit))
            {
                ShowBar(unit);
            }
            else
            {
                HideBar(unit.SpawnIndex);
            }
        }
    }

    // 스펙 9절: 피해를 입은 유닛에만 띄운다 — 전부 띄우면 80기 화면이 지저분해진다
    private bool NeedsBar(UnitController unit)
    {
        if (unit.IsDead) return false;
        if (unit.Stats == null) return false;
        return unit.Hp < unit.Stats.Hp;
    }

    private void ShowBar(UnitController unit)
    {
        Vector3 worldPoint = unit.transform.position + Vector3.up * barHeightOffset;
        Vector3 screenPoint = worldCamera.WorldToScreenPoint(worldPoint);

        // z가 음수면 카메라 뒤 — 그대로 그리면 화면 반대편에 유령 바가 생긴다
        if (screenPoint.z <= 0f)
        {
            HideBar(unit.SpawnIndex);
            return;
        }

        HealthBar bar = AcquireBar(unit.SpawnIndex);
        bar.SetScreenPosition(screenPoint);
        bar.SetRatio(unit.Hp / unit.Stats.Hp);
    }

    private HealthBar AcquireBar(int spawnIndex)
    {
        HealthBar bar;
        if (bars.TryGetValue(spawnIndex, out bar)) return bar;

        bar = barPool.Get().GetComponent<HealthBar>();
        bar.transform.SetParent(hudRoot, false);
        bars.Add(spawnIndex, bar);
        return bar;
    }

    private void HideBar(int spawnIndex)
    {
        HealthBar bar;
        if (!bars.TryGetValue(spawnIndex, out bar)) return;

        bars.Remove(spawnIndex);
        barPool.Release(bar.gameObject);
    }

    // 유닛이 죽어 풀로 돌아가면 로스터에서 빠져 위 순회에 안 잡힌다 — 남은 바를 여기서 걷어낸다
    private void ReleaseOrphanBars()
    {
        orphanBuffer.Clear();
        foreach (KeyValuePair<int, HealthBar> entry in bars)
        {
            if (GameManager.Units.Find(entry.Key) != null) continue;
            orphanBuffer.Add(entry.Key);
        }

        for (int i = 0; i < orphanBuffer.Count; i++)
        {
            HideBar(orphanBuffer[i]);
        }
    }

    // 풀이 부르는 세 콜백 — 템플릿 CreateObjectPool이 함수를 인자로 받는다
    private GameObject CreateBar()
    {
        return Instantiate(healthBarPrefab, hudRoot);
    }

    private void OnGetFromPool(GameObject bar)
    {
        bar.SetActive(true);
    }

    private void OnReleaseToPool(GameObject bar)
    {
        bar.SetActive(false);
    }

    public override void Clear()
    {
        orphanBuffer.Clear();
        foreach (KeyValuePair<int, HealthBar> entry in bars)
        {
            orphanBuffer.Add(entry.Key);
        }

        for (int i = 0; i < orphanBuffer.Count; i++)
        {
            HideBar(orphanBuffer[i]);
        }
        bars.Clear();
    }
}
