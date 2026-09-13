namespace ExcelManagement.Domain;

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
}
