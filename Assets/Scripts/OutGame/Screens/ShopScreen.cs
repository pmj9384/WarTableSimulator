using UnityEngine;
using UnityEngine.UI;

// 결제 창 껍데기. 가챠 로직을 걷어내고(스펙 13절 "가챠는 출시본에 없다") 재사용 가치가 있는 것만 남겼다 —
// UIScreen 수명주기 · 버튼 · 코인 변동 구독(재화 부족 방어의 자리).
// 안쪽은 현금 상품(골드 팩 · 광고 제거)으로 교체 예정이라 필드명은 씬 배선이 끊기지 않게 그대로 둔다.
public class ShopScreen : UIScreen
{
    [SerializeField] Button drawOneButton;
    [SerializeField] Button drawTenButton;

    public override void Open()
    {
        base.Open();
        OnCoinsChanged(0);
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged += OnCoinsChanged;
    }

    public override void Close()
    {
        base.Close();
        if (!GameDataManager.HasInstance) return;   // 부팅 Setup/teardown — Instance 접근이 초기화 전 생성을 유발하므로 금지
        GameDataManager.Instance.PlayerAccountData.OnCoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int _)
    {
        // TODO(결제): 상품 가격과 대조해 interactable 판정 — 가챠가 CanDrawOne/CanDrawTen으로 하던 자리
    }
}
