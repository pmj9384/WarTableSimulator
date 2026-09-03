using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace Game.Core.Data
{
    // UnitTable.csv 텍스트 → 유닛 스탯 목록. 순수 함수 — 파일/Resources 접근은 호출측 몫 (코스모 SkillTableParser 관례).
    // 파싱은 CsvHelper(프로젝트 공용 DLL)에 위임하고, 스키마 매핑과 게임 규칙 검증만 여기서 통제한다.
    // 실패는 전부 FormatException으로 감싼다 — 호출측과 테스트가 CsvHelper 타입을 몰라도 되게.
    public static class UnitTableParser
    {
        public static List<UnitStats> Parse(string csvText)
        {
            if (string.IsNullOrWhiteSpace(csvText))
                throw new FormatException("UnitTable: 내용이 비어 있다");

            var units = new List<UnitStats>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);  // 사람이 손으로 치는 값 — Grunt/grunt를 같은 id로 본다

            try
            {
                using var reader = new StringReader(csvText);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<UnitStatsMap>();

                foreach (UnitStats row in csv.GetRecords<UnitStats>())
                {
                    // Parser.Row = 원본 파일 기준 행 번호(헤더 = 1행) — 기획이 CSV에서 바로 찾아갈 수 있는 값
                    int line = csv.Context.Parser.Row;

                    if (!seenIds.Add(row.UnitId))
                        throw new FormatException($"UnitTable {line}행: unitId 중복 ({row.UnitId})");
                    ValidateRanges(row, line);

                    units.Add(row);
                }
            }
            catch (HeaderValidationException ex)
            {
                throw new FormatException("UnitTable: 헤더가 스키마와 다르다 — " + FirstLine(ex.Message));
            }
            catch (CsvHelperException ex)
            {
                int line = ex.Context?.Parser?.Row ?? 0;
                throw new FormatException($"UnitTable {line}행: 값 형식 오류 — " + FirstLine(ex.Message));
            }

            if (units.Count == 0)
                throw new FormatException("UnitTable: 데이터 행이 없다 (헤더만 있음)");

            return units;
        }

        // 게임 규칙 검증 — 0이나 음수가 섞이면 전투 계산(DPS·사거리)이 조용히 무너지므로 로드 시점에 막는다
        private static void ValidateRanges(UnitStats u, int line)
        {
            if (string.IsNullOrWhiteSpace(u.UnitId))
                throw new FormatException($"UnitTable {line}행: unitId가 비어 있다");
            if (u.PopCost < 1)
                throw new FormatException($"UnitTable {line}행: popCost는 1 이상 ({u.PopCost})");
            if (u.Hp <= 0)
                throw new FormatException($"UnitTable {line}행: hp는 양수 ({u.Hp})");
            if (u.Atk < 0)
                throw new FormatException($"UnitTable {line}행: atk는 0 이상 ({u.Atk})");
            if (u.AtkInterval <= 0)
                throw new FormatException($"UnitTable {line}행: atkInterval은 양수 ({u.AtkInterval})");
            if (u.AtkRange <= 0)
                throw new FormatException($"UnitTable {line}행: atkRange는 양수 ({u.AtkRange})");
            if (u.MoveSpeed <= 0)
                throw new FormatException($"UnitTable {line}행: moveSpeed는 양수 ({u.MoveSpeed})");
            if (u.AoeRadius < 0)
                throw new FormatException($"UnitTable {line}행: aoeRadius는 0 이상 ({u.AoeRadius})");
            if (u.HealPerSec < 0)
                throw new FormatException($"UnitTable {line}행: healPerSec은 0 이상 ({u.HealPerSec})");
            if (u.HealRadius < 0)
                throw new FormatException($"UnitTable {line}행: healRadius는 0 이상 ({u.HealRadius})");
            if (u.StructureMult < 1)
                throw new FormatException($"UnitTable {line}행: structureMult는 1 이상 ({u.StructureMult})");
        }

        // CsvHelper 예외 메시지는 여러 줄 안내문이라 첫 줄만 남긴다
        private static string FirstLine(string message)
        {
            int cut = message.IndexOf('\n');
            return cut < 0 ? message : message.Substring(0, cut).TrimEnd('\r');
        }

        // CSV 헤더(camelCase) ↔ C# 프로퍼티(PascalCase) 명시 매핑 — 자동 추론에 안 맡긴다
        private sealed class UnitStatsMap : ClassMap<UnitStats>
        {
            public UnitStatsMap()
            {
                Map(u => u.UnitId).Name("unitId");
                Map(u => u.Role).Name("role");
                Map(u => u.PopCost).Name("popCost");
                Map(u => u.Hp).Name("hp");
                Map(u => u.Atk).Name("atk");
                Map(u => u.AtkInterval).Name("atkInterval");
                Map(u => u.AtkRange).Name("atkRange");
                Map(u => u.MoveSpeed).Name("moveSpeed");
                Map(u => u.AoeRadius).Name("aoeRadius");
                Map(u => u.StructureMult).Name("structureMult");
                Map(u => u.HealPerSec).Name("healPerSec");
                Map(u => u.HealRadius).Name("healRadius");
                Map(u => u.DisplayName).Name("displayName");
                Map(u => u.Description).Name("description");
                Map(u => u.Icon).Name("icon");
            }
        }
    }
}
