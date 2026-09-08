using UnityEngine;
using UnityEngine.UI;

// 체력바 한 개. 화면 캔버스 위에 떠 있고, 자기 위치도 남은 비율도 모른다 — HudManager가 매 프레임 넣어준다.
// 유닛에 붙지 않는 이유: 유닛마다 캔버스를 달면 80기에서 캔버스가 80개가 된다 (캔버스 하나 = 리빌드 단위).
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (fill != null) return;

        // 자동 탐색을 안 하는 이유: 루트에도 Image(배경)가 있어서 그걸 집어버린다
        Debug.LogWarning($"{name}: 체력바 fill 이미지가 비어 있습니다. 프리팹에서 연결하세요.");
    }

    public void SetRatio(float ratio)
    {
        if (fill == null) return;
        fill.fillAmount = Mathf.Clamp01(ratio);
    }

    public void SetScreenPosition(Vector2 screenPoint)
    {
        rect.position = screenPoint;
    }
}
