namespace NhaGiaKim.Application.Common;

/// <summary>
/// Danh sach key cau hinh duoc phep xuat hien tren endpoint cong khai.
/// Dung chung cho ca noi ghi (AdminContentService) va noi doc (LandingService),
/// de khong bao gio co key nao luu duoc ma lai khong duoc kiem soat khi tra ra ngoai.
/// </summary>
public static class PublicSettingKeys
{
    public const string Logo = "site.logo";
    public const string Hotline = "contact.hotline";
    public const string Email = "contact.email";
    public const string Address = "contact.address";
    public const string Facebook = "social.facebook";
    public const string Youtube = "social.youtube";
    public const string FooterText = "footer.text";
    public const string BankInfo = "payment.bankInfo";

    public static readonly string[] All =
    [
        Logo, Hotline, Email, Address, Facebook, Youtube, FooterText, BankInfo
    ];
}
