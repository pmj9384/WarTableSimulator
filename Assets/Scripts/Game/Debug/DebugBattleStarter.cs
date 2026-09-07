using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 1주차 리트머스용 임시 도구 — 스펙 5절 상성 대결을 인게임에서 재현해 시뮬 예측과 대조한다.
// 기준: Grunt×3 (코스트 3) vs Guardian (3) → Guardian 승, 91HP 잔존, 20.6초 (스펙 5절 재계산본).
// 2주차 배치 조작이 들어오면 삭제한다.
public class DebugBattleStarter : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private readonly List<UnitController> spawned = new List<UnitController>();
    private bool battleRunning;
    private float elapsed;

    private void Start()
    {
        gameManager.Match.BattleEnded += ReportResult;
    }

    private void OnDestroy()
    {
        if (gameManager != null && gameManager.Match != null)
        {
            gameManager.Match.BattleEnded -= ReportResult;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) StartMatch(3, "Grunt", "Guardian");   // 1: 리트머스 (Grunt×3 vs Guardian)
        if (keyboard.digit2Key.wasPressedThisFrame) StartMatch(1, "Grunt", "Grunt");      // 2: 1v1 회귀 확인
        if (keyboard.rKey.wasPressedThisFrame) ClearField();

        if (battleRunning)
        {
            elapsed += Time.deltaTime;
        }
    }

    // 왼쪽에 팀0 여러 기, 오른쪽에 팀1 한 기 — 겹치지 않게 z축으로 벌려 놓는다
    private void StartMatch(int leftCount, string leftUnitId, string rightUnitId)
    {
        ClearField();

        for (int i = 0; i < leftCount; i++)
        {
            float z = (i - (leftCount - 1) * 0.5f) * 2f;
            spawned.Add(gameManager.Units.Spawn(leftUnitId, 0, new Vector3(-6f, 0f, z)));
        }
        spawned.Add(gameManager.Units.Spawn(rightUnitId, 1, new Vector3(6f, 0f, 0f)));

        gameManager.Match.StartBattle();
        battleRunning = true;
        elapsed = 0f;
        Debug.Log($"[리트머스] {leftUnitId}×{leftCount} vs {rightUnitId} 시작");
    }

    private void ClearField()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null && spawned[i].gameObject.activeSelf)
            {
                gameManager.Units.Despawn(spawned[i]);
            }
        }
        spawned.Clear();
        battleRunning = false;
    }

    // BattleManager가 전멸을 판정하면 MatchManager를 거쳐 여기로 온다 — 결과를 시뮬 예측과 비교할 수 있게 남긴다
    private void ReportResult(int winnerTeam)
    {
        if (!battleRunning) return;
        battleRunning = false;

        for (int i = 0; i < spawned.Count; i++)
        {
            UnitController unit = spawned[i];
            if (unit == null || !unit.gameObject.activeSelf) continue;
            Debug.Log($"[리트머스] 생존 {unit.SpawnIndex}번(팀{unit.Team}) HP {unit.Hp:F1} / 최대 {unit.Stats.Hp}");
        }
        Debug.Log($"[리트머스] 팀{winnerTeam} 승 · 경과 {elapsed:F1}초");
    }
}
