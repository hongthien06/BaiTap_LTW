using Microsoft.EntityFrameworkCore;

namespace NhaGiaKim.Application.Abstractions;

/// <summary>
/// Phan loai loi ghi DB. Tang Application khong duoc biet provider nao dang chay,
/// nen viec nhan dien ma loi cu the duoc day xuong Infrastructure.
/// </summary>
public interface IDbExceptionClassifier
{
    /// <summary>True khi loi la vi pham rang buoc duy nhat (unique index/constraint).</summary>
    bool IsUniqueConstraintViolation(DbUpdateException exception);
}
