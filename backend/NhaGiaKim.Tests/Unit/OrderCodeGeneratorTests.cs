using System.Text.RegularExpressions;
using NhaGiaKim.Application.Common;
using Shouldly;

namespace NhaGiaKim.Tests.Unit;

public class OrderCodeGeneratorTests
{
    [Fact]
    public void Generate_MatchesRequiredFormat()
    {
        var code = OrderCodeGenerator.Generate(new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc));
        Regex.IsMatch(code, @"^NGK-20261004-[A-Z2-9]{4}$").ShouldBeTrue($"ma sinh ra: {code}");
    }

    [Fact]
    public void Generate_ProducesDistinctCodes()
    {
        var now = DateTime.UtcNow;
        var codes = Enumerable.Range(0, 2_000).Select(_ => OrderCodeGenerator.Generate(now)).ToList();

        // Khong doi tuyet doi khong trung (unique index trong DB lo phan do),
        // nhung ty le trung phai rat thap de retry co y nghia.
        var distinct = codes.Distinct().Count();
        distinct.ShouldBeGreaterThan((int)(codes.Count * 0.95));
    }

    [Fact]
    public void Generate_ExcludesAmbiguousCharacters()
    {
        var now = DateTime.UtcNow;
        for (var i = 0; i < 500; i++)
        {
            var suffix = OrderCodeGenerator.Generate(now)[^4..];
            suffix.ShouldNotContain("O");
            suffix.ShouldNotContain("I");
            suffix.ShouldNotContain("0");
            suffix.ShouldNotContain("1");
        }
    }
}
