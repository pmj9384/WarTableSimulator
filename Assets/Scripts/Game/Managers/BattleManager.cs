using System.Collections.Generic;
using Game.Core.AI;
using Game.Core.Combat;
using UnityEngine;

// 전투 실행자 — 유저 결정(09-03)으로 세 책임을 한 몸에 두되, 이음새를 region으로 표시해 나중에 자를 수 있게 했다.
// 판단·계산은 전부 순수 코어(UnitTreeBuilder·TargetSelector·CombatRules·VictoryRules) 몫 — 여기는 "언제 부르나"만.
public class BattleManager : InGameManager
{
    private const float SuddenDeathStartSec = 45f;    // 스펙 7절 — 교착 방지 지속 피해 개시
    private const float RetargetIntervalSec = 0.3f;   // 플랜 Task 5 — 타겟 캐시 갱신 주기 (0.2~0.5 범위)

    private BehaviorNode sharedTree;   // 전 유닛이 공유하는 트리 인스턴스 하나 — "같은 행동 트리" 축
    private float battleElapsed;
    private float retargetTimer;
    private bool running;

    public override void Initialize()
    {
        sharedTree = UnitTreeBuilder.Build();
    }

    public void StartBattle()
    {
        battleElapsed = 0f;
        retargetTimer = 0f;
        running = true;
    }

    public void StopBattle()
    {
        running = false;
    }

    #region 심장 — 고정 스텝 틱 [분할 후보: BattleTicker]
    // FixedUpdate = 고정 스텝(기본 0.02s): 기기가 달라도 틱 수·순서가 같아야 결과가 같다 (멀티 전제)
    private void FixedUpdate()
    {
        if (!running) return;

        float dt = Time.fixedDeltaTime;
        battleElapsed += dt;

        UpdateTargets(dt);

        // 스폰 순번 순회 = 결정적 순서. 죽은 유닛은 틱에서 제외 (BT 안에 사망 체크 없음 — 스펙 11절)
        for (int team = 0; team < 2; team++)
        {
            IReadOnlyList<UnitController> roster = GameManager.Units.Roster(team);
            for (int i = 0; i < roster.Count; i++)
            {
                UnitController unit = roster[i];
                if (unit.IsDead) continue;

                if (battleElapsed >= SuddenDeathStartSec)
                    unit.TakeDamage(CombatRules.SuddenDeathTick(unit.Stats.Hp, dt));

                unit.TickTimers(dt);
                sharedTree.Tick(unit);

                if (unit.IsDead)
                    deadBuffer.Add(unit);   // 순회 도중 반환 금지 — 컬렉션이 흔들린다 (틱 끝 일괄)
            }
        }

        for (int i = 0; i < deadBuffer.Count; i++)
        {
            GameManager.Units.Despawn(deadBuffer[i]);
        }
        deadBuffer.Clear();

        // 전멸은 틱 안에서 일어나는 사건이라 그 자리에서 판정하고, 판을 끝내는 건 국면 담당에게 넘긴다
        int winnerTeam;
        if (IsBattleOver(out winnerTeam))
        {
            GameManager.Match.EndBattle(winnerTeam);
        }
    }

    private readonly List<UnitController> deadBuffer = new List<UnitController>();
    #endregion

    #region 눈 — 타겟 캐시 [분할 후보: TargetingManager]
    // 매 틱이 아니라 0.3s 주기: 80기 × 80기 전수 거리 계산을 틱마다 하면 낭비 (플랜의 캐시 서비스)
    private void UpdateTargets(float dt)
    {
        retargetTimer -= dt;
        if (retargetTimer > 0f) return;
        retargetTimer = RetargetIntervalSec;

        for (int team = 0; team < 2; team++)
        {
            IReadOnlyList<UnitController> enemies = GameManager.Units.Roster(1 - team);

            // 산 적만 후보로 — 정의역(누가 후보인가)은 호출자 책임 (TargetSelector 계약)
            candidateBuffer.Clear();
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].IsDead) continue;
                Vector3 p = enemies[i].transform.position;
                candidateBuffer.Add(new TargetInfo(enemies[i].SpawnIndex, p.x, p.z, TargetKind.Unit));
            }

            IReadOnlyList<UnitController> roster = GameManager.Units.Roster(team);
            for (int i = 0; i < roster.Count; i++)
            {
                UnitController unit = roster[i];
                if (unit.IsDead) continue;
                Vector3 my = unit.transform.position;
                int targetIndex = TargetSelector.SelectNearest(my.x, my.z, candidateBuffer);
                unit.SetTarget(targetIndex >= 0 ? GameManager.Units.Find(targetIndex) : null);
            }
        }
    }

    private readonly List<TargetInfo> candidateBuffer = new List<TargetInfo>();  // 매 갱신 재사용 — 틱마다 새 리스트 금지
    #endregion

    #region 심판 — 전멸 판정 [계산은 이미 VictoryRules(순수)로 분리됨]
    // 카운트 목록은 VictoryRules가 정의 — 1주차는 유닛만 있으므로 로스터의 생존 수만 센다
    public bool IsBattleOver(out int winnerTeam)
    {
        int alive0 = CountAlive(0);
        int alive1 = CountAlive(1);

        winnerTeam = alive0 == 0 ? 1 : 0;
        return alive0 == 0 || alive1 == 0;
    }

    private int CountAlive(int team)
    {
        IReadOnlyList<UnitController> roster = GameManager.Units.Roster(team);
        int count = 0;
        for (int i = 0; i < roster.Count; i++)
        {
            // 유닛은 전부 카운트 대상 (VictoryRules: Unit=true) — 구조물이 생기면 여기서 Kind로 거른다
            if (!roster[i].IsDead) count++;
        }
        return count;
    }
    #endregion
}
