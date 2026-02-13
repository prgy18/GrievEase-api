namespace GrievEase.API.Constants;

public static class GrievanceStatus
{
    public const string Pending = "pending";
    public const string InProcess = "in process";
    public const string Solved = "solved";

    // Validation helper
    public static bool IsValid(string status)
    {
        return status == Pending
            || status == InProcess
            || status == Solved;
    }

    // Get all valid statuses (useful for error messages)
    public static string[] GetAllStatuses()
    {
        return new[] { Pending, InProcess, Solved };
    }
}