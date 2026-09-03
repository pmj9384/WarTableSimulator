using Game.Core.Combat;

namespace Game.Core.AI
{
    // 전 유닛 공통 행동 트리의 조립처 — 8종이 이 트리 "인스턴스 하나"를 공유한다 (판정값만 Stats로 다름).
    // 우선순위 사다리: ①교전(칠 수 있으면 쳐) ②접근(타겟 있으면 가) ③재탐색(없으면 찾아).
    // 2주차에 구조물 분기(길 막힘 → 벽 공격)가 ①과 ② 사이에 끼어든다.
    public static class UnitTreeBuilder
    {
        public static BehaviorNode Build()
        {
            return new SelectorNode(

                // ① 교전 — 타겟이 있고 사거리 안이면 친다 (판단은 CombatRules에 위임)
                new SequenceNode(
                    new ConditionNode(ctx => ctx.HasTarget
                        && CombatRules.IsInRangeSq(ctx.DistanceToTargetSq, ctx.Stats.AtkRange)),
                    new ActionNode(ctx => ctx.Attack())),

                // ② 접근 — 타겟은 있는데 멀면 다가간다
                new SequenceNode(
                    new ConditionNode(ctx => ctx.HasTarget),
                    new ActionNode(ctx => ctx.MoveToTarget())),

                // ③ 재탐색 — 타겟이 없으면 새로 찾아달라고 요청
                new ActionNode(ctx => ctx.RequestRetarget()));
        }
    }
}
