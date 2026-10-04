using NhaGiaKim.Application.Abstractions;
using NhaGiaKim.Application.Common;

namespace NhaGiaKim.Infrastructure.Storage;

public class LocalFileStorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Thu muc vat ly chua file tai len, vi du wwwroot/uploads.</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Tien to URL public, vi du /uploads.</summary>
    public string PublicBaseUrl { get; set; } = "/uploads";
}

public class LocalFileStorage(LocalFileStorageOptions options) : IFileStorage
{
    private const int HeaderSize = 16;

    public async Task<StoredFile> SaveAsync(Stream content, UploadKind kind, CancellationToken ct = default)
    {
        var maxBytes = FileSignatureValidator.MaxBytes(kind);
        if (content.CanSeek && content.Length > maxBytes)
            throw new ArgumentException($"File vuot qua gioi han {maxBytes / 1024 / 1024} MB.");

        var header = new byte[HeaderSize];
        var read = await content.ReadAsync(header.AsMemory(0, HeaderSize), ct);
        var extension = FileSignatureValidator.Detect(header.AsSpan(0, read), kind)
            ?? throw new ArgumentException("Dinh dang file khong hop le.");

        if (content.CanSeek) content.Position = 0;

        // Ten file ngau nhien: khong giu ten goc do client gui (chong path traversal + trung ten).
        var fileName = $"{Guid.NewGuid():N}{extension}";
        Directory.CreateDirectory(options.RootPath);
        var fullPath = Path.Combine(options.RootPath, fileName);

        await using (var target = File.Create(fullPath))
        {
            if (content.CanSeek)
            {
                await content.CopyToAsync(target, ct);
            }
            else
            {
                await target.WriteAsync(header.AsMemory(0, read), ct);
                await content.CopyToAsync(target, ct);
            }

            if (target.Length > maxBytes)
            {
                target.Close();
                File.Delete(fullPath);
                throw new ArgumentException($"File vuot qua gioi han {maxBytes / 1024 / 1024} MB.");
            }
        }

        var size = new FileInfo(fullPath).Length;
        return new StoredFile($"{options.PublicBaseUrl.TrimEnd('/')}/{fileName}", fileName, size);
    }
}
