using NhaGiaKim.Application.Abstractions;

namespace NhaGiaKim.Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    // Tinh mot lan cho ca tien trinh: moi lan verify deu ton dung mot phep BCrypt.
    private static readonly string DummyPasswordHash =
        BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), WorkFactor);

    public string DummyHash => DummyPasswordHash;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash hong trong DB -> coi nhu sai mat khau, khong nem exception ra ngoai.
            return false;
        }
    }
}
