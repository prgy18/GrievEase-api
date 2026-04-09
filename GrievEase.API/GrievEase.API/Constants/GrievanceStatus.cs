namespace GrievEase.API.Constants;

public static class GrievanceStatus
{
    public const string Pending = "pending";
    public const string InProcess = "in process";
    public const string Solved = "solved";
    public const string AwaitingApproval = "awaiting approval";

    // Validation helper
    public static bool IsValid(string status) =>
    status == Pending || status == InProcess ||
    status == AwaitingApproval || status == Solved;

    public static IEnumerable<string> GetAllStatuses() =>
        new[] { Pending, InProcess, AwaitingApproval, Solved };
}