namespace Game.Core.Combat
{
    // 전멸 판정의 "누굴 세는가" — 서술어가 아니라 이 목록이 기준이다 (스펙 7-4, 08-27 정정).
    // 세는 것: 유닛·Turret. 안 세는 것: Decoy·Barricade (그리고 Mine — 2주차 구조물 때 종류 추가 예정).
    // 지속 피해(45s~)의 대상도 이 목록과 동일 — "밟을 적 없는 지뢰만 남는 교착"을 막는 규칙.
    public static class VictoryRules
    {
        public static bool CountsForElimination(TargetKind kind)
        {
            return kind == TargetKind.Unit || kind == TargetKind.Turret;
        }
    }
}
