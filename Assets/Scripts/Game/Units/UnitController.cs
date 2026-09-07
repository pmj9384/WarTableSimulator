using Game.Core.AI;
using Game.Core.Combat;
using Game.Core.Data;
using UnityEngine;
using UnityEngine.AI;

// 유닛 한 기의 신원·상태 + BT 계약(IUnitContext) 구현.
// 구조 결정(09-07, 유저): 타겟은 참조+스폰인덱스 쌍(재사용 검증), 타이머는 BattleManager 틱이 깎고,
// 죽은 유닛은 틱 끝에 일괄 반환. 프리팹 인스턴스는 매니저를 모른다 — 값과 명령만 받는다.
public class UnitController : MonoBehaviour, IUnitContext
{
    public int SpawnIndex { get; private set; }
    public int Team { get; private set; }
    public float Hp { get; private set; }
    public UnitStats Stats { get; private set; }
    public bool IsDead => CombatRules.IsDead(Hp);

    private UnitController targetRef;   // 위치를 읽기 위한 참조
    private int targetSpawnIndex = -1;  // 참조의 진위 보증 — 풀 재사용되면 어긋난다
    private float atkTimer;             // 0 이하 = 쏠 수 있음. 깎는 건 BattleManager 틱
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Setup(int spawnIndex, int team, UnitStats stats, Vector3 position)
    {
        SpawnIndex = spawnIndex;
        Team = team;
        Stats = stats;
        Hp = stats.Hp;
        transform.position = position;

        atkTimer = 0f;                  // 재장전 상태로 시작 — "사거리 진입 즉시 1타"의 근원
        targetRef = null;               // 이전 생의 타겟이 남으면 안 된다 (재사용 객체는 상태가 남는다)
        targetSpawnIndex = -1;
        agent.speed = stats.MoveSpeed;
    }

    public void TakeDamage(float amount)
    {
        Hp -= amount;
    }

    // BattleManager 눈이 호출 — 타겟 교체
    public void SetTarget(UnitController target)
    {
        targetRef = target;
        targetSpawnIndex = target != null ? target.SpawnIndex : -1;
    }

    // BattleManager 틱이 호출 — 시간은 여기서만 흐른다
    public void TickTimers(float deltaTime)
    {
        atkTimer -= deltaTime;
    }

    // ── IUnitContext 계약 (BT가 부름)
    public bool HasTarget
    {
        get
        {
            if (targetRef == null) return false;
            if (targetRef.SpawnIndex != targetSpawnIndex) return false;   // 몸이 재사용됨 — 남남이다
            return !targetRef.IsDead;
        }
    }

    public float DistanceToTargetSq
    {
        get
        {
            if (!HasTarget) return float.MaxValue;
            float dx = targetRef.transform.position.x - transform.position.x;
            float dz = targetRef.transform.position.z - transform.position.z;
            return dx * dx + dz * dz;   // x/z 평면 — TargetSelector·사거리 판정과 같은 거리 정의
        }
    }

    public void MoveToTarget()
    {
        agent.isStopped = false;
        agent.SetDestination(targetRef.transform.position);
    }

    public void Attack()
    {
        agent.isStopped = true;         // 멈춰서 친다 (이동·공격 동시 금지)
        if (atkTimer > 0f) return;      // 아직 재장전 중

        float damage = CombatRules.DamageAgainst(Stats.Atk, Stats.StructureMult, targetIsStructure: false);
        targetRef.TakeDamage(damage);
        atkTimer = Stats.AtkInterval;   // 다음 발사까지 간격 — 발사 관례의 후반부
    }

    public void RequestRetarget()
    {
        targetRef = null;               // 비워두면 다음 눈 주기(0.3s)가 새로 채운다
        targetSpawnIndex = -1;
    }
}
