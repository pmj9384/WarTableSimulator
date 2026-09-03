namespace Game.Core.AI
{
    public enum NodeState { Success, Failure, Running }
    // Running은 1주차엔 안 쓴다 — 재평가식(매 틱 루트부터)이라 노드가 진행 상태를 기억할 일이 없다.
    // 선딜·캐스팅 같은 "여러 틱짜리 행동"이 생기면 그때 쓴다.

    // BT 노드의 뿌리 — 무상태가 규칙이다: 노드는 아무것도 기억하지 않고,
    // 유닛의 상태는 전부 ctx(IUnitContext) 쪽에 있다. 그래서 트리 인스턴스 하나를 전 유닛이 공유한다.
    public abstract class BehaviorNode
    {
        public abstract NodeState Tick(IUnitContext ctx);
    }
}
