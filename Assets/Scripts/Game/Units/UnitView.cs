using UnityEngine;

// 유닛의 겉모습 담당 — 지금은 팀 색뿐이고, 앞으로 피격 깜빡임·사망 연출이 여기로 온다.
// 전투 판단(UnitController)과 나눠 둔 이유: 표현이 늘어도 컨트롤러가 커지지 않게 하려고.
// 스펙 9절 "팀은 색, 역할은 실루엣" 중 색을 맡는다 (실루엣은 프리팹이 갈린다).
public class UnitView : MonoBehaviour
{
    [SerializeField] private Renderer bodyRenderer;

    private void Awake()
    {
        if (bodyRenderer != null) return;
        bodyRenderer = GetComponentInChildren<Renderer>();   // 프리팹에서 빠뜨려도 몸통을 찾아 쓴다
    }

    // 스폰할 때 UnitManager가 팀 머티리얼을 넘겨준다 — 프리팹 인스턴스는 매니저를 모르고 값만 받는다
    public void ApplyTeam(Material teamMaterial)
    {
        if (bodyRenderer == null) return;
        if (teamMaterial == null) return;

        // sharedMaterial: 머티리얼 인스턴스를 새로 만들지 않아 같은 팀끼리 배칭이 유지된다
        bodyRenderer.sharedMaterial = teamMaterial;
    }
}
