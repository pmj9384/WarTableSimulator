using System.Collections.Generic;
using UnityEngine;

// Task 5 검증용 임시 도구 — 스폰/반환이 풀로 도는지 눈으로 확인한다.
// Task 7의 DebugBattleStarter(N:N 하드코딩 스폰)로 대체되면 삭제.
public class DebugSpawnTester : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private readonly List<UnitController> spawned = new List<UnitController>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))   // 스페이스: 양 팀 Grunt 1기씩 스폰
        {
            spawned.Add(gameManager.Units.Spawn("Grunt", 0, new Vector3(-5f, 0f, 0f)));
            spawned.Add(gameManager.Units.Spawn("Grunt", 1, new Vector3(5f, 0f, 0f)));
            Debug.Log($"[DebugSpawn] 스폰 — 현재 {spawned.Count}기");
        }

        if (Input.GetKeyDown(KeyCode.R))       // R: 전부 반환 (풀로 되돌아가는지)
        {
            foreach (UnitController unit in spawned)
                gameManager.Units.Despawn(unit);
            spawned.Clear();
            Debug.Log("[DebugSpawn] 전부 반환");
        }
    }
}
