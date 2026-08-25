# War Table Simulator — 프로젝트 규약

포트폴리오 2호작. 3D 유닛 전투 시뮬레이터 — 인구 상한 안에서 유닛·구조물을 배치하고
**개입 없이 관전**해 전멸시키는 게임. 4주 개발 + Google Play 출시 + 업데이트 1~2회.

**유저가 모든 코드를 면접에서 설명할 수 있어야 하므로, 유저의 이해가 결과물의 일부다.**
혼자 달리지 말 것 — 승인 게이트 밖에서 코드 파일을 수정하지 않는다.

작업 리듬·승인 게이트·설계 원칙의 SSOT = `~/.claude/rules/` (유저 스코프, 자동 로드).
운영 절차의 SSOT = `/dev-cycle` 스킬. 이 파일엔 **프로젝트 특수사항만** 남긴다.

## 구현의 기준 문서

**`~/Documents/Obsidian Vault/설계문서/신규게임-유닛시뮬/스펙.md` (493줄)가 SSOT다.**
전장 크기·유닛 8종 스탯·상성·구조물 6종·전투 규칙·배치 조작·아웃게임·멀티 로드맵이 전부 여기 있다.
같은 폴더의 다른 문서(기획-스냅샷·유닛-밸런스-초안·AI-아키텍처-초안·레퍼런스-조사·설계-감사)는
**근거 자료**이고, 스펙과 어긋나면 스펙을 따른다.

스펙 부록에 "앞선 문서를 정정한 목록" 10건이 있다 — 옛 문서를 인용하기 전에 거기부터 본다.

## 이 게임의 축 (설계 판단이 갈릴 때의 기준)

- **플레이어가 만지는 레버는 배치 하나뿐이다.** 어떤 기능이든 "배치의 무게를 늘리는가"로 판정한다
- **막혔을 때의 답은 항상 "배치를 다시 짠다"여야 한다.** 레벨업·인구 상한 구매를 넣지 않는 이유가 이것
- **전 유닛이 같은 행동 트리를 쓴다.** 판정값만 다르다. 역할마다 트리를 새로 만들면 이 전제가 깨진다
- **타겟은 "가장 가까운 적" 하나뿐이다.** 정의역(무엇이 타겟 목록에 드는가)은 스펙 7-1에 표로 있다

## 코드의 출신

이 프로젝트는 **커스텀 템플릿**(`com.pmj.template.mobilegame`)에서 생성됐다. 그래서 코드가 두 부류다.

- **템플릿 출신** — `Scripts/Core`(매니저 허브·세이브·풀·로딩), `Scripts/OutGame`, `Scripts/UISystem`,
  `Scripts/Data`, `Editor/`. 여기를 고치면 **템플릿에도 역이식할지** 판단한다
  (기준: "프로젝트가 달라도 하는 일이 같은가")
- **게임 코드** — 앞으로 만드는 전투·유닛·배치·스테이지. 템플릿에 안 올라간다

**안 쓰는 아웃게임 모듈**: 스태미나 · 가챠 · 스킨. 이 게임은 셋 다 안 쓴다(스펙 13절).
아직 안 뗐고, 떼려면 모듈당 코드 7파일을 건드려야 한다 — 코인·세이브·매니저와 얽혀 있다.
`partial` 분리로 "폴더 삭제만으로 제거"가 되게 하는 작업이 예정돼 있다.

**코인은 쓴다.** 골드로 유닛·구조물을 해금한다.

## 검증 — Unity CLI (MCP 아님)

Unity가 에디터 내장 MCP 서버를 폐기하고 CLI로 옮겼다. `unity` 명령을 Bash로 직접 부른다.

```bash
unity projects verify .    # meta 누락·무결성
unity run .                # 임포트 + 컴파일 (exit 0이어야 함)
unity test . --mode EditMode
```

- **에디터가 열려 있으면 배치 모드가 거부된다.** "another Unity instance is running"이 나오면 에디터를 닫는다
- 상세 컴파일 오류는 **첫 실행에만** CLI로 나온다. 이후는 로그를 본다:
  `grep -oE "Assets/[^(]*\([0-9]+,[0-9]+\): error CS[0-9]+: .*" "$(ls -t ~/Library/Logs/Unity/Editor*.log | head -1)" | sort -u`
- **컴파일·verify가 통과해도 프리팹·씬의 죽은 스크립트 참조는 못 잡는다.** Play에서 Missing Script로
  나타난다. 프리팹을 건드렸으면 `m_Script` GUID를 `Assets` 안 meta와 대조한다
  (단, 패키지 스크립트를 오탐하지 않게 남은 GUID는 `Library/PackageCache`에서 다시 찾아본다)
- **동작 검증은 유저 Play Mode.** 체크리스트를 제시한다

## 프로젝트 관례

- 매니저: `InGameManager` 상속 + 태그 `"Manager"` + `GameManager` 프로퍼티·등록 — 3종 세트.
  **아웃게임은 이 패턴이 아니다** — `OutGameManager`가 따로 돈다
- 매니저 간 참조는 `GameManager.{Manager}` 경유. 프리팹 인스턴스는 매니저를 모르고 값만 받는다
- 오브젝트 생성은 ObjectPool. `FindObjectOfType`/`GameObject.Find` 금지
- **AI·이동·전투는 고정 스텝(`FixedUpdate`)으로 돈다.** 기기가 느려도 결과가 같아야 하고,
  나중에 붙일 멀티의 전제이기도 하다
- **플레이어 배치는 스테이지의 적 배치와 같은 데이터 형식으로 저장한다.** 재도전·진형 저장에 쓰이고,
  멀티에서 그대로 서버에 올린다
- 데이터는 CSV: `UnitTable.csv` · `StructureTable.csv` · `StageComposition.csv`
- 커밋: 한국어 + why 포함 + 논리 단위 분할. 구현 → 유저 Play 확인 → 즉시 커밋
- **브랜치**: `feature/*` → `develop`. **`master`는 빌드 검증(AAB)된 시점에만**

## 빌드·출시

- `Tools/Build/Android APK` (로컬·테스터 배포) · `Tools/Build/Android AAB` (스토어 제출)
- 패키지명·제품명·화면 방향은 `Assets/Editor/BuildScript.cs` 상단 상수가 **빌드마다 강제**한다.
  인스펙터 값에 의존하지 않는다. 패키지명 `com.pmj.wartablesimulator`는 스토어 등록 후 못 바꾼다
- **Play Console 개인 계정(2026년 생성)은 프로덕션 전 테스터 12명·14일 연속이 필요하다.**
  3주차에 돌아가는 빌드가 나오면 **즉시** 비공개 테스트에 올려 14일을 개발과 병행한다.
  4주를 다 쓰고 시작하면 3주가 뒤에 붙는다

## 테스트 가능 경계

MonoBehaviour·NavMesh 의존이 없는 판정은 순수 C#으로 뺀다(코스모의 `TongTong.Core` 방식).

- **EditMode 대상**: 최근접 타겟 선정(동률 타이브레이크·빈 리스트·사거리 밖), 데미지/광역 반경/
  구조물 배수, 지속 피해 계산
- **PlayMode·수동**: BT 흐름, NavMeshAgent 이동, Animator. 단 노드 안의 판단은
  `CombatRules.IsInRange(distance, range)` 같은 순수 함수로 빼고 노드는 얇은 어댑터로 둔다

## 레퍼런스 스킬 (이름만 알지 말고 실제 호출)

- 플랜 문서 작성 시 `superpowers:writing-plans`, 완료 선언 전 `superpowers:verification-before-completion`,
  구현 후 리뷰 시 `superpowers:requesting-code-review`
- 프로젝트 세팅 절차는 `unity-project-setup` 스킬 (이 프로젝트가 그걸로 만들어졌다)
