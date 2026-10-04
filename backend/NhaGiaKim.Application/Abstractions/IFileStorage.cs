using NhaGiaKim.Application.Common;

namespace NhaGiaKim.Application.Abstractions;

public record StoredFile(string Url, string FileName, long Size);

public interface IFileStorage
{
    /// <summary>Luu file voi ten ngau nhien. Nem ArgumentException neu file khong hop le.</summary>
    Task<StoredFile> SaveAsync(Stream content, UploadKind kind, CancellationToken ct = default);
}
