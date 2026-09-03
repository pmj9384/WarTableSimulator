using Game.Core.AI;
using Game.Core.Data;
using NUnit.Framework;

// 행동 트리 검증 — 가짜 유닛(FakeUnitContext)에게 상황을 주고, 트리가 "어떤 명령을 내렸는가"만 본다.
// 트리는 무상태라 인스턴스 하나를 전 유닛이 공유한다 — 마지막 테스트가 그 축을 증명한다.
public class UnitTreeTests
{
    // 기록장 달린 가짜 유닛 — BT가 시킨 횟수를 센다. 엔진 없이 트리를 검증하는 열쇠
    private class FakeUnitContext : IUnitContext
    {
        public UnitStats Stats { get; set; } = new UnitStats { AtkRange = 1.5f };
        public bool HasTarget { get; set; }
        public float DistanceToTargetSq { get; set; }

        public int AttackCalls, MoveCalls, RetargetCalls;
        public void Attack() => AttackCalls++;
        public void MoveToTarget() => MoveCalls++;
        public void RequestRetarget() => RetargetCalls++;
    }

    [Test]
    public void 사거리_안이면_공격만_한다()
    {
        var unit = new FakeUnitContext { HasTarget = true, DistanceToTargetSq = 1.5f * 1.5f };  // 정확히 경계

        UnitTreeBuilder.Build().Tick(unit);

        Assert.AreEqual(1, unit.AttackCalls, "경계 = 사거리 안 (진입 즉시 1타)");
        Assert.AreEqual(0, unit.MoveCalls);
        Assert.AreEqual(0, unit.RetargetCalls);
    }

    [Test]
    public void 타겟이_사거리_밖이면_접근만_한다()
    {
        var unit = new FakeUnitContext { HasTarget = true, DistanceToTargetSq = 100f };

        UnitTreeBuilder.Build().Tick(unit);

        Assert.AreEqual(0, unit.AttackCalls);
        Assert.AreEqual(1, unit.MoveCalls);
        Assert.AreEqual(0, unit.RetargetCalls);
    }

    [Test]
    public void 타겟이_없으면_재탐색만_한다()
    {
        var unit = new FakeUnitContext { HasTarget = false };

        UnitTreeBuilder.Build().Tick(unit);

        Assert.AreEqual(0, unit.AttackCalls);
        Assert.AreEqual(0, unit.MoveCalls);
        Assert.AreEqual(1, unit.RetargetCalls);
    }

    [Test]
    public void 트리_하나를_여러_유닛이_공유해도_서로_안_섞인다()
    {
        // "전 유닛이 같은 행동 트리" 축의 증명 — 같은 인스턴스로 상태 다른 두 유닛을 틱
        BehaviorNode sharedTree = UnitTreeBuilder.Build();
        var fighter = new FakeUnitContext { HasTarget = true, DistanceToTargetSq = 1f };
        var searcher = new FakeUnitContext { HasTarget = false };

        sharedTree.Tick(fighter);
        sharedTree.Tick(searcher);
        sharedTree.Tick(fighter);   // 교차로 다시 틱 — 노드에 상태가 남아 있으면 여기서 오염된다

        Assert.AreEqual(2, fighter.AttackCalls);
        Assert.AreEqual(1, searcher.RetargetCalls);
        Assert.AreEqual(0, fighter.RetargetCalls);
        Assert.AreEqual(0, searcher.AttackCalls);
    }
}
