using TMPro;
using UnityEngine;

// 게임 제목 라벨 — 인스펙터 문자열 대신 Application.productName을 넣는다.
// BuildScript가 ProductName을 빌드마다 상수로 강제하므로 제목도 같은 출처를 보게 해서
// 템플릿에서 찍은 프로젝트마다 제목이 갈라지지 않게 한다.
[RequireComponent(typeof(TMP_Text))]
public class ProductNameLabel : MonoBehaviour
{
    private void Awake() => GetComponent<TMP_Text>().text = Application.productName;
}
