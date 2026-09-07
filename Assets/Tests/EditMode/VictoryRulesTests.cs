using Game.Core.Combat;
using NUnit.Framework;

// 전멸 카운트 목록 검증 — 스펙 7-4의 목록 정의를 그대로 박제.
// 이 목록이 바뀌면(예: 2주차 Mine 추가) 여기 테스트도 같이 바뀌어야 한다 — 그게 의도.
public class VictoryRulesTests
{
    [Test]
    public void 유닛과_터렛만_전멸_카운트에_든다()
    {
        Assert.IsTrue(VictoryRules.CountsForElimination(TargetKind.Unit));
        Assert.IsTrue(VictoryRules.CountsForElimination(TargetKind.Turret));
        Assert.IsFalse(VictoryRules.CountsForElimination(TargetKind.Decoy), "미끼만 남으면 전투는 끝난 것");
        Assert.IsFalse(VictoryRules.CountsForElimination(TargetKind.Barricade), "벽만 남으면 전투는 끝난 것");
    }
}
