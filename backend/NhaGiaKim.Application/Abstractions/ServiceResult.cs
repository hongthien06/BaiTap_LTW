namespace NhaGiaKim.Application.Abstractions;

/// <summary>Ket qua tra ve tu service: thanh cong kem data, hoac that bai kem ma loi + loi field.</summary>
public class ServiceResult<T>
{
    public bool Succeeded { get; private init; }
    public T? Value { get; private init; }
    public ServiceErrorCode ErrorCode { get; private init; }
    public string? ErrorMessage { get; private init; }
    public IReadOnlyDictionary<string, string[]> Errors { get; private init; } =
        new Dictionary<string, string[]>();

    public static ServiceResult<T> Ok(T value) => new() { Succeeded = true, Value = value };

    public static ServiceResult<T> Fail(ServiceErrorCode code, string message,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new()
        {
            Succeeded = false,
            ErrorCode = code,
            ErrorMessage = message,
            Errors = errors ?? new Dictionary<string, string[]>()
        };

    public static ServiceResult<T> Invalid(string field, string message) =>
        Fail(ServiceErrorCode.Validation, message,
            new Dictionary<string, string[]> { [field] = [message] });
}

public enum ServiceErrorCode
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}
