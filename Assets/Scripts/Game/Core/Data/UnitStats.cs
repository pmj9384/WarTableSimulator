namespace Game.Core.Data
{
    // 유닛 1종의 판정값 스키마 — UnitTable.csv 한 행과 1:1 (스펙 4절 수치 + 12절 스키마 15컬럼).
    // 스키마의 주인은 이 클래스다: CSV 컬럼을 늘리려면 여기부터 늘린다.
    // struct가 아니라 class인 이유 — CsvHelper가 리플렉션으로 프로퍼티를 채우는 방식과 맞추기 위함.
    public class UnitStats
    {
        // ── 전투 수치 (인게임 소비)
        public string UnitId { get; set; }
        public string Role { get; set; }           // 역할 표기 (물량·탱커…) — 해금 창 표시용
        public int PopCost { get; set; }
        public float Hp { get; set; }
        public float Atk { get; set; }
        public float AtkInterval { get; set; }
        public float AtkRange { get; set; }
        public float MoveSpeed { get; set; }
        public float AoeRadius { get; set; }       // 0 = 단일 대상 (Mortar만 3)
        public float StructureMult { get; set; }   // 구조물 피해 배수, 1 = 배수 없음 (Breacher만 8)
        public float HealPerSec { get; set; }      // 출시본 전부 0 — Support 업데이트 대비 (스펙 12절 "남겨둔다")
        public float HealRadius { get; set; }

        // ── 표시 텍스트 (아웃게임 해금 창 소비, 3주차에 채움)
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}
