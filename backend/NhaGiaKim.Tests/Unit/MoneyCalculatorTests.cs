using NhaGiaKim.Application.Common;
using Shouldly;

namespace NhaGiaKim.Tests.Unit;

public class MoneyCalculatorTests
{
    [Fact]
    public void EffectiveUnitPrice_UsesDiscountWhenValid()
        => MoneyCalculator.EffectiveUnitPrice(89_000m, 69_000m).ShouldBe(69_000m);

    [Fact]
    public void EffectiveUnitPrice_IgnoresNullDiscount()
        => MoneyCalculator.EffectiveUnitPrice(89_000m, null).ShouldBe(89_000m);

    // Gia giam >= gia goc la du lieu rac -> khong duoc lam tang gia ban.
    [Theory]
    [InlineData(89_000, 89_000)]
    [InlineData(89_000, 99_000)]
    [InlineData(89_000, 0)]
    [InlineData(89_000, -1)]
    public void EffectiveUnitPrice_IgnoresNonsenseDiscount(decimal price, decimal discount)
        => MoneyCalculator.EffectiveUnitPrice(price, discount).ShouldBe(price);

    [Theory]
    [InlineData(69_000, 1, 69_000)]
    [InlineData(69_000, 2, 138_000)]
    [InlineData(69_000, 99, 6_831_000)]
    public void Total_MultipliesUnitPriceByQuantity(decimal unit, int qty, decimal expected)
        => MoneyCalculator.Total(unit, qty).ShouldBe(expected);

    [Fact]
    public void Total_RoundsToTwoDecimals()
        => MoneyCalculator.Total(10.005m, 1).ShouldBe(10.01m);
}
