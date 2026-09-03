using System.Collections.Generic;
using Game.Core.Data;
using UnityEngine;

// UnitTable 접근층 — 템플릿 DataTableManager 관례와 동형(정적·지연 로드·캐시).
// 파싱·검증은 순수 코어(UnitTableParser) 몫, 여기는 Resources에서 꺼내 캐시하는 문지기만 한다.
public static class UnitTableRepository
{
    private const string ResourcePath = "Tables/UnitTable";
    private static Dictionary<string, UnitStats> byId;

    public static UnitStats Get(string unitId)
    {
        EnsureLoaded();
        if (!byId.TryGetValue(unitId, out UnitStats stats))
            Debug.LogError($"[UnitTableRepository] 없는 unitId: {unitId}");
        return stats;
    }

    public static IReadOnlyCollection<UnitStats> All
    {
        get { EnsureLoaded(); return byId.Values; }
    }

    private static void EnsureLoaded()
    {
        if (byId != null) return;

        var csv = Resources.Load<TextAsset>(ResourcePath);
        if (csv == null)
        {
            Debug.LogError($"[UnitTableRepository] {ResourcePath} 없음");
            byId = new Dictionary<string, UnitStats>();
            return;
        }

        byId = new Dictionary<string, UnitStats>();
        foreach (UnitStats stats in UnitTableParser.Parse(csv.text))
            byId.Add(stats.UnitId, stats);
    }
}
