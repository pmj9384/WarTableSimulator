using Game.Core.AI;
using Game.Core.Combat;
using Game.Core.Data;
using UnityEngine;

// 유닛 한 기의 신원·상태 + BT 계약(IUnitContext) 구현.
// Task 5 단계 = 스폰 확인용 스텁: 계약의 "몸"(이동·공격·타겟 보관)은 Task 6에서 채운다.
public class UnitController : MonoBehaviour, IUnitContext
{
    public int SpawnIndex { get; private set; }
    public int Team { get; private set; }
    public float Hp { get; private set; }
    public UnitStats Stats { get; private set; }
    public bool IsDead => CombatRules.IsDead(Hp);

    // 풀에서 나올 때마다 새 신원을 받는다 — 재사용 객체는 상태가 남는다는 코스모 교훈의 실천
    public void Setup(int spawnIndex, int team, UnitStats stats, Vector3 position)
    {
        SpawnIndex = spawnIndex;
        Team = team;
        Stats = stats;
        Hp = stats.Hp;
        transform.position = position;
    }

    public void TakeDamage(float amount)
    {
        Hp -= amount;
    }

    // ── IUnitContext 계약 (Task 6에서 몸을 채움 — 지금은 컴파일 가능한 최소)
    public bool HasTarget => false;              // TODO(Task 6): BattleManager가 캐시해준 타겟 보관
    public float DistanceToTargetSq => float.MaxValue;
    public void Attack() { }                     // TODO(Task 6): 발사 관례(진입 즉시 1타 + atkInterval)
    public void MoveToTarget() { }               // TODO(Task 6): NavMeshAgent SetDestination
    public void RequestRetarget() { }            // TODO(Task 6): BattleManager에 재탐색 요청
}
