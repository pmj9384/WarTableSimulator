using Game.Core.Combat;
using NUnit.Framework;

// CombatRules 검증 — 케이스는 4주 플랜 Task 2에서 확정.
// 주의: 플랜의 "Breacher 구조물 64"는 DPS(12×8÷1.5s)고, 한 방 피해는 96이다 — 테스트는 한 방 기준.
public class CombatRulesTests
{
    // ── IsInRange: 스펙 4-1 "사거리 진입 즉시 1타" → 경계는 포함(<=)
    [Test]
    public void 사거리_경계값은_사거리_안이다()
    {
        Assert.IsTrue(CombatRules.IsInRange(1.5f, 1.5f), "정확히 사거리 = 공격 가능");
        Assert.IsTrue(CombatRules.IsInRange(1.4f, 1.5f));
        Assert.IsFalse(CombatRules.IsInRange(1.5001f, 1.5f), "사거리 밖 = 공격 불가");
    }

    // ── DamageAgainst: 구조물 배수는 구조물에만 (스펙 4절 Breacher atk 12, structureMult 8)
    [Test]
    public void 구조물_배수는_구조물을_칠_때만_붙는다()
    {
        Assert.AreEqual(12f, CombatRules.DamageAgainst(12f, 8f, targetIsStructure: false), "유닛 상대 = 원래 atk");
        Assert.AreEqual(96f, CombatRules.DamageAgainst(12f, 8f, targetIsStructure: true), "구조물 상대 = atk×8 (DPS로는 96÷1.5s = 64)");
        Assert.AreEqual(8f, CombatRules.DamageAgainst(8f, 1f, targetIsStructure: true), "배수 1이면 구조물이어도 그대로 (Grunt)");
    }

    // ── SuddenDeathTick: 45초 이후 초당 최대 HP의 4% (스펙 7절) → 25초면 어떤 유닛이든 전멸
    [Test]
    public void 지속피해는_초당_최대HP의_4퍼센트다()
    {
        Assert.AreEqual(420f * 0.04f * 0.02f, CombatRules.SuddenDeathTick(420f, 0.02f), 1e-5f, "Guardian 고정 스텝 1틱");
    }

    [Test]
    public void 지속피해_25초_누적이면_최대HP와_같다()
    {
        // 고정 스텝(0.02s)으로 25초 = 1250틱 누적 — 부동소수 누적 오차 허용 ±0.5
        float total = 0f;
        for (int i = 0; i < 1250; i++)
            total += CombatRules.SuddenDeathTick(420f, 0.02f);
        Assert.AreEqual(420f, total, 0.5f);
    }

    // ── IsDead: "HP ≤ 0 = 사망" 경계를 함수로 못박음 — Mine 90 vs HP 90이 반드시 죽어야 한다 (스펙 부록 정정: 지뢰 피해 90)
    [Test]
    public void 피해가_HP와_정확히_같아도_죽는다()
    {
        float hpAfterMine = 90f - CombatRules.DamageAgainst(90f, 1f, false);   // Grunt(80)보다 큰 90 가정
        Assert.IsTrue(CombatRules.IsDead(hpAfterMine), "HP 0 = 사망 (0 이하 판정)");
        Assert.IsFalse(CombatRules.IsDead(0.0001f), "털끝만큼 남으면 생존");
        Assert.IsTrue(CombatRules.IsDead(-5f));
    }
}
