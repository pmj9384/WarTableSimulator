using System;
using System.Collections.Generic;
using System.IO;
using Game.Core.Data;
using NUnit.Framework;

// UnitTableParser 검증 — 케이스 목록은 09-03 설계 대화에서 확정, 리뷰 권고로 검증 분기·중복 케이스 보강.
// ①정상 파싱 값 일치 ②실제 Resources CSV와 스펙 4절 전건 대조(수동 대조를 기계로 대체)
// ③헤더 불일치 ④빈 텍스트 ⑤깨진 숫자(행 번호) ⑥중복 unitId(행 번호·대소문자 무시)
// ⑦컬럼 수 부족 ⑧범위 검증 분기(경계값 5종)
public class UnitTableParserTests
{
    private const string ValidHeader =
        "unitId,role,popCost,hp,atk,atkInterval,atkRange,moveSpeed,aoeRadius,structureMult,healPerSec,healRadius,displayName,description,icon";

    private const string GruntRow = "Grunt,물량,1,80,8,1.0,1.5,3.5,0,1,0,0,Grunt,,";

    private const string ValidCsv = ValidHeader + "\n" +
        GruntRow + "\n" +
        "Breacher,공성,2,130,12,1.5,1.5,3.4,0,8,0,0,Breacher,,\n";

    [Test]
    public void 정상CSV_값이_그대로_매핑된다()
    {
        List<UnitStats> units = UnitTableParser.Parse(ValidCsv);

        Assert.AreEqual(2, units.Count);
        Assert.AreEqual("Grunt", units[0].UnitId);
        Assert.AreEqual("물량", units[0].Role);
        Assert.AreEqual(1, units[0].PopCost);
        Assert.AreEqual(80f, units[0].Hp);
        Assert.AreEqual(8f, units[0].Atk);
        Assert.AreEqual(1.0f, units[0].AtkInterval);
        Assert.AreEqual(1.5f, units[0].AtkRange);
        Assert.AreEqual(3.5f, units[0].MoveSpeed);
        Assert.AreEqual(0f, units[0].AoeRadius);
        Assert.AreEqual(1f, units[0].StructureMult);
        Assert.AreEqual(0f, units[0].HealPerSec);
        Assert.AreEqual("Grunt", units[0].DisplayName);
        Assert.AreEqual("", units[0].Description);   // 빈칸 허용 — 3주차에 채움
        Assert.AreEqual(8f, units[1].StructureMult); // Breacher 공성 배수
    }

    [Test]
    public void 실제_Resources_CSV가_스펙4절과_전건_일치한다()
    {
        // 수동 대조(완료 기준)를 기계 검사로 대체 — 파일이 스펙과 어긋나면 CI가 잡는다
        string csv = File.ReadAllText("Assets/Resources/Tables/UnitTable.csv");
        List<UnitStats> units = UnitTableParser.Parse(csv);

        Assert.AreEqual(8, units.Count, "출시 유닛은 8종 (Support 제외)");
        AssertUnit(units[0], "Grunt",      "물량",      1,  80,  8, 1.0f,  1.5f, 3.5f, 0, 1);
        AssertUnit(units[1], "Striker",    "근접 딜러", 2, 140, 22, 0.8f,  1.5f, 3.8f, 0, 1);
        AssertUnit(units[2], "Guardian",   "탱커",      3, 420, 14, 1.2f,  1.8f, 3.0f, 0, 1);
        AssertUnit(units[3], "Marksman",   "원거리",    2,  90, 30, 1.5f,  8f,   3.2f, 0, 1);
        AssertUnit(units[4], "Mortar",     "광역",      3, 110, 16, 2.0f, 10f,   2.8f, 3, 1);
        AssertUnit(units[5], "Skirmisher", "기동",      2, 100, 18, 1.0f,  1.5f, 6.0f, 0, 1);
        AssertUnit(units[6], "Breacher",   "공성",      2, 130, 12, 1.5f,  1.5f, 3.4f, 0, 8);
        AssertUnit(units[7], "Sniper",     "저격",      3,  70, 70, 3.0f, 16f,   2.6f, 0, 1);
    }

    [Test]
    public void 헤더가_스키마와_다르면_예외()
    {
        string csv = ValidHeader.Replace("hp", "HP오타") + "\n" + GruntRow + "\n";

        var ex = Assert.Throws<FormatException>(() => UnitTableParser.Parse(csv));
        StringAssert.Contains("헤더", ex.Message);
    }

    [Test]
    public void 빈_텍스트면_예외()
    {
        Assert.Throws<FormatException>(() => UnitTableParser.Parse(""));
        Assert.Throws<FormatException>(() => UnitTableParser.Parse("   \n  "));
    }

    [Test]
    public void 숫자가_깨진_행은_행번호를_알려준다()
    {
        string csv = ValidHeader + "\n" + GruntRow + "\n" +
                     "Striker,근접 딜러,2,abc,22,0.8,1.5,3.8,0,1,0,0,Striker,,\n";   // 3행 hp 깨짐

        var ex = Assert.Throws<FormatException>(() => UnitTableParser.Parse(csv));
        StringAssert.Contains("3행", ex.Message);
    }

    [Test]
    public void 컬럼이_모자란_행은_예외()
    {
        string csv = ValidHeader + "\n" + GruntRow + "\n" +
                     "Striker,근접 딜러,2,140\n";   // 3행: 15컬럼 중 4개뿐

        Assert.Throws<FormatException>(() => UnitTableParser.Parse(csv));
    }

    [Test]
    public void 중복_unitId는_대소문자를_무시하고_행번호를_알려준다()
    {
        string csv = ValidHeader + "\n" + GruntRow + "\n" +
                     "grunt,물량,2,140,22,0.8,1.5,3.8,0,1,0,0,grunt,,\n";   // 3행 = 2행과 대소문자만 다른 중복

        var ex = Assert.Throws<FormatException>(() => UnitTableParser.Parse(csv));
        StringAssert.Contains("3행", ex.Message);
        StringAssert.Contains("중복", ex.Message);
    }

    // 범위 검증 8분기의 대표 경계값 — DPS(atk÷interval) 같은 파생 계산이 조용히 무너지는 값들을 로드에서 막는지
    [TestCase("Grunt,물량,0,80,8,1.0,1.5,3.5,0,1,0,0,,,",   "popCost")]   // popCost 0
    [TestCase("Grunt,물량,1,0,8,1.0,1.5,3.5,0,1,0,0,,,",    "hp")]        // hp 0
    [TestCase("Grunt,물량,1,80,8,0,1.5,3.5,0,1,0,0,,,",     "atkInterval")] // interval 0 → 무한 DPS
    [TestCase("Grunt,물량,1,80,8,1.0,1.5,3.5,0,0.5,0,0,,,", "structureMult")] // 배수 1 미만
    [TestCase(",물량,1,80,8,1.0,1.5,3.5,0,1,0,0,,,",        "unitId")]    // id 공백
    public void 범위를_벗어난_값은_필드명과_행번호를_알려준다(string badRow, string fieldName)
    {
        string csv = ValidHeader + "\n" + badRow + "\n";

        var ex = Assert.Throws<FormatException>(() => UnitTableParser.Parse(csv));
        StringAssert.Contains("2행", ex.Message);
        StringAssert.Contains(fieldName, ex.Message);
    }

    private static void AssertUnit(UnitStats u, string id, string role, int pop, float hp, float atk,
        float interval, float range, float speed, float aoe, float structMult)
    {
        Assert.AreEqual(id, u.UnitId);
        Assert.AreEqual(role, u.Role, $"{id}.role");
        Assert.AreEqual(pop, u.PopCost, $"{id}.popCost");
        Assert.AreEqual(hp, u.Hp, $"{id}.hp");
        Assert.AreEqual(atk, u.Atk, $"{id}.atk");
        Assert.AreEqual(interval, u.AtkInterval, $"{id}.atkInterval");
        Assert.AreEqual(range, u.AtkRange, $"{id}.atkRange");
        Assert.AreEqual(speed, u.MoveSpeed, $"{id}.moveSpeed");
        Assert.AreEqual(aoe, u.AoeRadius, $"{id}.aoeRadius");
        Assert.AreEqual(structMult, u.StructureMult, $"{id}.structureMult");
        Assert.AreEqual(0f, u.HealPerSec, $"{id}.healPerSec — 출시본은 전부 0");
        Assert.AreEqual(0f, u.HealRadius, $"{id}.healRadius — 출시본은 전부 0");
    }
}
