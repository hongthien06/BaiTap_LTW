namespace NhaGiaKim.Application.Common;

public static class MoneyCalculator
{
    /// <summary>Gia ban thuc te: uu tien gia giam neu hop le.</summary>
    public static decimal EffectiveUnitPrice(decimal price, decimal? discountPrice)
        => discountPrice is > 0 && discountPrice < price ? discountPrice.Value : price;

    public static decimal Total(decimal unitPrice, int quantity)
        => decimal.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);
}
