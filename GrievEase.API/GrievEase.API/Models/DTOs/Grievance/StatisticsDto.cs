namespace GrievEase.API.Models.DTOs.Grievance;

public class StatisticsDto
{
    // Overall counts
    public int TotalGrievances { get; set; }
    public int PendingGrievances { get; set; }
    public int InProcessGrievances { get; set; }
    public int SolvedGrievances { get; set; }

    // Average resolution time in days
    public double AverageResolutionDays { get; set; }

    // Grouped by department
    public List<DepartmentStats> DepartmentWiseStats { get; set; } = new();

    // Top 10 localities with most grievances
    public List<LocalityStats> TopLocalities { get; set; } = new();
}

public class DepartmentStats
{
    public string Department { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProcess { get; set; }
    public int Solved { get; set; }
}

public class LocalityStats
{
    public string Locality { get; set; } = string.Empty;
    public int TotalGrievances { get; set; }
    public int SolvedGrievances { get; set; }
}