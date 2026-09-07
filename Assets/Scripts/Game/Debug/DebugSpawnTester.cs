using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Task 6 검증용 임시 도구 — 스폰/전투 시작/반환을 키로 확인한다.
// 입력은 Input System 패키지 기준 — 이 프로젝트는 구형 Input 클래스를 쓰면 예외를 던진다(Player Settings).
// Task 7의 DebugBattleStarter(N:N 하드코딩 스폰)로 대체되면 삭제.
public class DebugSpawnTester : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private readonly List<UnitController> spawned = new List<UnitController>();

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)   // 스페이스: 양 팀 Grunt 1기씩 스폰
        {
            spawned.Add(gameManager.Units.Spawn("Grunt", 0, new Vector3(-5f, 0f, 0f)));
            spawned.Add(gameManager.Units.Spawn("Grunt", 1, new Vector3(5f, 0f, 0f)));
            Debug.Log($"[DebugSpawn] 스폰 — 현재 {spawned.Count}기");
        }

        if (keyboard.bKey.wasPressedThisFrame)       // B: 전투 시작 — 틱·타겟팅·발사가 이때부터 돈다
        {
            gameManager.Match.StartBattle();
            Debug.Log("[DebugSpawn] 전투 시작");
        }

        if (keyboard.rKey.wasPressedThisFrame)       // R: 전부 반환 (풀로 되돌아가는지)
        {
            foreach (UnitController unit in spawned)
            {
                gameManager.Units.Despawn(unit);
            }
            spawned.Clear();
            Debug.Log("[DebugSpawn] 전부 반환");
        }
    }
}
