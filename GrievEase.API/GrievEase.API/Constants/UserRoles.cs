namespace GrievEase.API.Constants;


public enum SignInType
{
    LocalityMember = 0,      // Regular citizen
    GovernmentOfficial = 1   // Government employee
}

public static class UserRoles
{
    public const string LocalityMember = "LocalityMember";
    public const string GovernmentOfficial = "GovernmentOfficial";
}
