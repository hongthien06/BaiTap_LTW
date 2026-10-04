namespace NhaGiaKim.Application.Abstractions;

/// <summary>
/// File tai len khong hop le (sai dinh dang, qua kich thuoc).
///
/// Dung exception rieng thay vi ArgumentException: neu middleware bat ArgumentException roi
/// tra 400 thi moi ArgumentNullException/ArgumentOutOfRangeException phat sinh o bat ky dau -
/// trong EF Core, trong thu vien ngoai - cung bi phan loai nham thanh "loi cua nguoi goi",
/// mat log va lo nguyen van thong diep noi bo ra client.
/// </summary>
public class FileValidationException(string message) : Exception(message);
