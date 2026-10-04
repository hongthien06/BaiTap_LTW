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
    private const int CopyBufferSize = 81_920;

    public async Task<StoredFile> SaveAsync(Stream content, UploadKind kind, CancellationToken ct = default)
    {
        var maxBytes = FileSignatureValidator.MaxBytes(kind);

        if (content.CanSeek && content.Length > maxBytes)
            throw new FileValidationException(TooLargeMessage(maxBytes));

        // ReadAsync chi bao dam tra ve >= 1 byte. WebP can 12 byte, PNG can 8 byte de nhan dang,
        // nen doc thieu se lam mot file hop le bi bao la sai dinh dang.
        var header = new byte[HeaderSize];
        var headerLength = await content.ReadAtLeastAsync(header, HeaderSize, throwOnEndOfStream: false, ct);

        var extension = FileSignatureValidator.Detect(header.AsSpan(0, headerLength), kind)
            ?? throw new FileValidationException("Dinh dang file khong hop le.");

        // Ten file ngau nhien voi duoi do server tu quyet theo magic bytes:
        // khong co byte nao tu client di vao duong dan -> khong the path traversal.
        var fileName = $"{Guid.NewGuid():N}{extension}";
        Directory.CreateDirectory(options.RootPath);
        var fullPath = Path.Combine(options.RootPath, fileName);

        try
        {
            await using var target = File.Create(fullPath);
            var written = await WriteLimitedAsync(content, target, header.AsMemory(0, headerLength), maxBytes, ct);

            return new StoredFile($"{options.PublicBaseUrl.TrimEnd('/')}/{fileName}", fileName, written);
        }
        catch
        {
            // Khong de lai file do dang tren dia khi ghi that bai hoac vuot gioi han.
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }
    }

    /// <summary>
    /// Ghi stream xuong dia va dung NGAY khi vuot gioi han, thay vi ghi het roi moi do kich thuoc.
    /// Voi stream khong seek duoc, kiem tra sau khi ghi dong nghia da cho phep ghi file lon tuy y.
    /// </summary>
    private static async Task<long> WriteLimitedAsync(
        Stream source, Stream target, ReadOnlyMemory<byte> alreadyRead, long maxBytes, CancellationToken ct)
    {
        long written = 0;

        if (alreadyRead.Length > 0)
        {
            await target.WriteAsync(alreadyRead, ct);
            written += alreadyRead.Length;
        }

        var buffer = new byte[CopyBufferSize];
        int read;
        while ((read = await source.ReadAsync(buffer, ct)) > 0)
        {
            written += read;
            if (written > maxBytes) throw new FileValidationException(TooLargeMessage(maxBytes));

            await target.WriteAsync(buffer.AsMemory(0, read), ct);
        }

        return written;
    }

    private static string TooLargeMessage(long maxBytes) =>
        $"File vuot qua gioi han {maxBytes / 1024 / 1024} MB.";
}
