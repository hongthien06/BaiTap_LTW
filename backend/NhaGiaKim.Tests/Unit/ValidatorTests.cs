using NhaGiaKim.Application.Dtos.Public;
using NhaGiaKim.Application.Validators;
using NhaGiaKim.Domain.Enums;
using Shouldly;

namespace NhaGiaKim.Tests.Unit;

public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _validator = new();

    private static CreateOrderRequest Valid(int quantity = 1, string phone = "0901234567") =>
        new("Nguyen Van A", phone, "123 Duong Sach, Quan 1, TPHCM", quantity, PaymentMethod.Cod, null);

    // AC-9: bien so luong.
    [Theory]
    [InlineData(1, true)]
    [InlineData(99, true)]
    [InlineData(0, false)]
    [InlineData(100, false)]
    [InlineData(-1, false)]
    public void Quantity_BoundaryValues(int quantity, bool expectedValid)
        => _validator.Validate(Valid(quantity)).IsValid.ShouldBe(expectedValid);

    // AC-8: dinh dang SDT.
    [Theory]
    [InlineData("0901234567", true)]
    [InlineData("0000000000", true)]
    [InlineData("901234567", false)]
    [InlineData("+84901234567", false)]
    [InlineData("0901234ABC", false)]
    [InlineData("09012345678", false)]
    [InlineData("", false)]
    public void Phone_FormatRules(string phone, bool expectedValid)
        => _validator.Validate(Valid(phone: phone)).IsValid.ShouldBe(expectedValid);

    [Fact]
    public void CustomerName_TooShortIsRejected()
        => _validator.Validate(Valid() with { CustomerName = "A" }).IsValid.ShouldBeFalse();

    [Fact]
    public void Address_TooShortIsRejected()
        => _validator.Validate(Valid() with { Address = "123 ABC" }).IsValid.ShouldBeFalse();

    [Fact]
    public void Note_OverLimitIsRejected()
        => _validator.Validate(Valid() with { Note = new string('x', 1001) }).IsValid.ShouldBeFalse();

    [Fact]
    public void PaymentMethod_OutOfEnumIsRejected()
        => _validator.Validate(Valid() with { PaymentMethod = (PaymentMethod)99 }).IsValid.ShouldBeFalse();
}

public class CreateFeedbackRequestValidatorTests
{
    private readonly CreateFeedbackRequestValidator _validator = new();

    private static CreateFeedbackRequest Valid(int rating = 5) => new("Tran Thi B", rating, "Sach hay");

    // AC-15: bien so sao.
    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(6, false)]
    [InlineData(-1, false)]
    public void Rating_BoundaryValues(int rating, bool expectedValid)
        => _validator.Validate(Valid(rating)).IsValid.ShouldBe(expectedValid);

    [Fact]
    public void Content_OverTwoThousandCharsIsRejected()
        => _validator.Validate(Valid() with { Content = new string('x', 2001) }).IsValid.ShouldBeFalse();

    [Fact]
    public void Content_EmptyIsRejected()
        => _validator.Validate(Valid() with { Content = "" }).IsValid.ShouldBeFalse();
}
