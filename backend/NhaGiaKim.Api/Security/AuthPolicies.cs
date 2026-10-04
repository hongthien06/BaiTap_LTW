namespace NhaGiaKim.Api.Security;

public static class AuthPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireStaffOrAdmin = "RequireStaffOrAdmin";
}

public static class CorsPolicies
{
    public const string Frontend = "Frontend";
}

public static class RateLimitPolicies
{
    public const string PublicWrite = "PublicWrite";
}
