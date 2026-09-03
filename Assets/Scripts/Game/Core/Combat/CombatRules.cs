namespace Game.Core.Combat
{
    // 전투 판정 순수 함수 모음 — 상태 없음, 엔진 없음. BT 노드(어댑터)는 판단을 전부 여기로 위임한다.
    // 스펙 근거: 4-1 발사 관례(사거리 진입 즉시 1타 → 경계 포함), 4절 구조물 배수, 7절 지속 피해(초당 최대 HP 4%).
    public static class CombatRules
    {
        // 45초 교착 방지 지속 피해의 초당 비율 (스펙 7절) — 25초면 어떤 유닛이든 전멸하는 값
        private const float SuddenDeathRatePerSec = 0.04f;

        // 경계 포함(<=): "사거리 진입 즉시 1타"라서 정확히 사거리에 닿은 순간도 공격 가능이어야 한다
        public static bool IsInRange(float distance, float range)
        {
            return distance <= range;
        }

        // 제곱거리 버전 — TargetSelector가 √ 없이 계산한 거리²를 그대로 판정에 쓴다 (의미는 IsInRange와 동일)
        public static bool IsInRangeSq(float distanceSq, float range)
        {
            return distanceSq <= range * range;
        }

        // 구조물 배수는 구조물을 칠 때만 — 유닛 상대로는 원래 atk (Breacher 12 / 벽엔 96)
        public static float DamageAgainst(float atk, float structureMult, bool targetIsStructure)
        {
            return targetIsStructure ? atk * structureMult : atk;
        }

        // 고정 스텝 1틱당 감쇠량 — 최대 HP 기준이라 잔여 HP와 무관하게 일정 속도로 깎인다
        public static float SuddenDeathTick(float maxHp, float deltaTime)
        {
            return maxHp * SuddenDeathRatePerSec * deltaTime;
        }

        // "HP ≤ 0 = 사망" 경계의 단일 정의처 — Mine 90 vs HP 90처럼 딱 떨어지는 피해도 반드시 죽는다
        public static bool IsDead(float hp)
        {
            return hp <= 0f;
        }
    }
}
