namespace GrievEase.API.Constants;

public static class Departments     //Static so that object need not to be created
{
    public const string WaterWorks = "Water-Works";
    public const string Roadways = "Roadways";
    public const string Electricity = "Electricity";
    public const string Sanitation = "Sanitation";
    public const string StreetLights = "Street-Lights";
    public const string Drainage = "Drainage";

    public static bool IsValid(string department)      //Used in DTO to check if input is valid or not 
    {
        return department == WaterWorks
            || department == Roadways
            || department == Electricity
            || department == Sanitation
            || department == StreetLights
            || department == Drainage;
    }

    public static string[] GetAllDepartments()  //used in error message
    {
        return new[]
        {
            WaterWorks,
            Roadways,
            Electricity,
            Sanitation,
            StreetLights,
            Drainage
        };
    }
}