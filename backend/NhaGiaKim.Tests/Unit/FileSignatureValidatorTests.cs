using NhaGiaKim.Application.Common;
using Shouldly;

namespace NhaGiaKim.Tests.Unit;

public class FileSignatureValidatorTests
{
    private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46];
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
    private static readonly byte[] PdfHeader = [0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37];
    private static readonly byte[] ExeHeader = [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00];

    private static byte[] WebpHeader()
    {
        var bytes = new byte[12];
        "RIFF"u8.CopyTo(bytes);
        "WEBP"u8.CopyTo(bytes.AsSpan(8));
        return bytes;
    }

    [Fact]
    public void Detect_RecognisesJpeg() =>
        FileSignatureValidator.Detect(JpegHeader, UploadKind.Image).ShouldBe(".jpg");

    [Fact]
    public void Detect_RecognisesPng() =>
        FileSignatureValidator.Detect(PngHeader, UploadKind.Image).ShouldBe(".png");

    [Fact]
    public void Detect_RecognisesWebp() =>
        FileSignatureValidator.Detect(WebpHeader(), UploadKind.Image).ShouldBe(".webp");

    [Fact]
    public void Detect_RecognisesPdf() =>
        FileSignatureValidator.Detect(PdfHeader, UploadKind.Pdf).ShouldBe(".pdf");

    // AC-29: file thuc thi doi duoi thanh .jpg phai bi tu choi.
    [Fact]
    public void Detect_RejectsExecutableDisguisedAsImage() =>
        FileSignatureValidator.Detect(ExeHeader, UploadKind.Image).ShouldBeNull();

    [Fact]
    public void Detect_RejectsImageWhenPdfExpected() =>
        FileSignatureValidator.Detect(JpegHeader, UploadKind.Pdf).ShouldBeNull();

    [Fact]
    public void Detect_RejectsPdfWhenImageExpected() =>
        FileSignatureValidator.Detect(PdfHeader, UploadKind.Image).ShouldBeNull();

    [Fact]
    public void Detect_RejectsTruncatedHeader() =>
        FileSignatureValidator.Detect([0xFF, 0xD8], UploadKind.Image).ShouldBeNull();

    [Fact]
    public void Detect_RejectsEmptyHeader() =>
        FileSignatureValidator.Detect([], UploadKind.Image).ShouldBeNull();

    [Fact]
    public void MaxBytes_DiffersByKind()
    {
        FileSignatureValidator.MaxBytes(UploadKind.Image).ShouldBe(5 * 1024 * 1024);
        FileSignatureValidator.MaxBytes(UploadKind.Pdf).ShouldBe(20 * 1024 * 1024);
    }
}
