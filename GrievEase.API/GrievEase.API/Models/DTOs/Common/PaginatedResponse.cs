namespace GrievEase.API.Models.DTOs.Common;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public static PaginatedResponse<T> Create(
        List<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords)
    {
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new PaginatedResponse<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1
        };
    }
}