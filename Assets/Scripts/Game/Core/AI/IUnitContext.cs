using Game.Core.Data;

namespace Game.Core.AI
{
    // BT가 유닛에게 묻고(읽기) 시키는(행동) 유일한 창구 — Task 5·6이 이 계약을 구현한다.
    // 러너 BT의 "T : MonoBehaviour" 제약을 인터페이스로 바꾼 지점: 가짜 구현만 있으면 엔진 없이 트리를 테스트한다.
    // 2주차 확장 예정: PathBlocked(벽 분기) — 지금 안 넣는 이유는 쓰지 않는 계약을 테스트 없이 방치하지 않기 위함.
    public interface IUnitContext
    {
        // ── 묻기
        UnitStats Stats { get; }
        bool HasTarget { get; }
        float DistanceToTargetSq { get; }

        // ── 시키기 (쿨다운·데미지 계산은 구현측(UnitController) 책임 — BT는 명령만)
        void Attack();
        void MoveToTarget();
        void RequestRetarget();
    }
}
