namespace ExcelManagement.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateOnly JoinDate { get; set; }
    public bool Status { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}
