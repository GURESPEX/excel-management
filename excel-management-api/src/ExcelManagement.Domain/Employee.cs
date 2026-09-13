namespace ExcelManagement.Domain;

public class Employee
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public decimal Salary { get; set; }
    public DateOnly JoinDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
