using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Task 5 씬 배선 도구 — "복잡한 조립은 에디터 API 도구로만" (코스모 손조립 사고 후 규약).
// 멱등: 이미 있는 것은 건너뛰므로 몇 번 눌러도 안전하다. 에디터 메뉴와 CLI 배치 모드 양쪽에서 실행 가능.
public static class BattleSceneWiring
{
    private const string ScenePath = "Assets/Scenes/InGameScene.unity";
    private const string PrefabDir = "Assets/Prefabs";
    private const string PrefabPath = PrefabDir + "/Unit_Temp.prefab";

    [MenuItem("Tools/Setup/Wire Battle Managers")]
    public static void Wire()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
            scene = EditorSceneManager.OpenScene(ScenePath);

        // ① 매니저 3종 — InGameManager 상속 + "Manager" 태그 관례
        EnsureManager<UnitManager>("UnitManager");
        EnsureManager<BattleManager>("BattleManager");
        EnsureManager<MatchManager>("MatchManager");

        // ② 임시 유닛 프리팹 (캡슐 + UnitController) — Task 6에서 Unit_Grunt로 대체
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Unit_Temp";
            capsule.AddComponent<UnitController>();
            prefab = PrefabUtility.SaveAsPrefabAsset(capsule, PrefabPath);
            Object.DestroyImmediate(capsule);
            Debug.Log("[Wiring] Unit_Temp 프리팹 생성");
        }

        // UnitManager의 직렬화 필드에 프리팹 연결 (private [SerializeField]라 SerializedObject 경유)
        var unitManager = Object.FindFirstObjectByType<UnitManager>();
        var so = new SerializedObject(unitManager);
        if (so.FindProperty("unitPrefab").objectReferenceValue == null)
        {
            so.FindProperty("unitPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            Debug.Log("[Wiring] UnitManager.unitPrefab 연결");
        }

        // ③ 스폰 테스터 + GameManager 참조
        var tester = Object.FindFirstObjectByType<DebugSpawnTester>();
        if (tester == null)
        {
            var testerGo = new GameObject("DebugSpawnTester");
            tester = testerGo.AddComponent<DebugSpawnTester>();
            Debug.Log("[Wiring] DebugSpawnTester 생성");
        }
        var gm = Object.FindFirstObjectByType<GameManager>();
        var testerSo = new SerializedObject(tester);
        if (testerSo.FindProperty("gameManager").objectReferenceValue == null)
        {
            testerSo.FindProperty("gameManager").objectReferenceValue = gm;
            testerSo.ApplyModifiedProperties();
            Debug.Log("[Wiring] DebugSpawnTester.gameManager 연결");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Wiring] 완료 — 씬 저장됨. Play에서 Space 스폰 / R 반환 확인");
    }

    // 이름으로 찾고, 없으면 생성 + 컴포넌트 + 태그까지 — 관례 3종 세트의 도구화
    private static void EnsureManager<T>(string name) where T : Component
    {
        if (Object.FindFirstObjectByType<T>() != null) return;
        var go = new GameObject(name);
        go.AddComponent<T>();
        go.tag = "Manager";
        Debug.Log($"[Wiring] {name} 생성 + Manager 태그");
    }
}
