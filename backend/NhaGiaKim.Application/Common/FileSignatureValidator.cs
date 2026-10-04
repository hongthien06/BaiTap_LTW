namespace NhaGiaKim.Application.Common;

public enum UploadKind { Image, Pdf }

/// <summary>
/// Kiem tra file bang MAGIC BYTES, khong tin duoi file hay Content-Type do client gui (AC-29).
/// </summary>
public static class FileSignatureValidator
{
    public const long MaxImageBytes = 5 * 1024 * 1024;
    public const long MaxPdfBytes = 20 * 1024 * 1024;

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Riff = [0x52, 0x49, 0x46, 0x46];       // RIFF....WEBP
    private static readonly byte[] Webp = [0x57, 0x45, 0x42, 0x50];
    private static readonly byte[] Pdf = [0x25, 0x50, 0x44, 0x46, 0x2D];  // %PDF-

    /// <summary>Tra ve extension chuan hoa neu hop le, nguoc lai tra null.</summary>
    public static string? Detect(ReadOnlySpan<byte> header, UploadKind kind)
    {
        if (kind == UploadKind.Pdf)
            return StartsWith(header, Pdf) ? ".pdf" : null;

        if (StartsWith(header, Jpeg)) return ".jpg";
        if (StartsWith(header, Png)) return ".png";
        if (header.Length >= 12 && StartsWith(header, Riff) && header[8..12].SequenceEqual(Webp)) return ".webp";
        return null;
    }

    public static long MaxBytes(UploadKind kind) => kind == UploadKind.Pdf ? MaxPdfBytes : MaxImageBytes;

    private static bool StartsWith(ReadOnlySpan<byte> header, ReadOnlySpan<byte> signature)
        => header.Length >= signature.Length && header[..signature.Length].SequenceEqual(signature);
}
