namespace NhaGiaKim.Application.Common;

/// <summary>
/// Kiem tra URL do admin nhap truoc khi cho hien thi tren landing.
/// Khong loc scheme thi mot chuoi dang javascript:... luu vao SourceUrl/FileUrl se duoc render
/// thang vao thuoc tinh href - khach bam vao la script chay, tuc stored XSS pham vi cong khai
/// phat ra tu mot quyen chi duoc phep sua van ban.
/// </summary>
public static class UrlRules
{
    public static bool IsSafeLink(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;

        // Duong dan noi bo kieu /uploads/abc.pdf - an toan, khong co scheme.
        if (url.StartsWith('/') && !url.StartsWith("//")) return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var parsed)
            && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
    }
}
