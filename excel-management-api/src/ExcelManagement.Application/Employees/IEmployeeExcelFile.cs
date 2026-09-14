namespace ExcelManagement.Application.Employees;

/// <summary>
/// Excel read/write for the Employee list. Kept as a plain-BCL-types interface here so
/// Application stays free of the Excel library type; the implementation (using ClosedXML)
/// lives in Infrastructure.
/// </summary>
public interface IEmployeeExcelFile
{
    byte[] WriteWorkbook(IReadOnlyList<EmployeeListItemDto> employees);

    IReadOnlyList<EmployeeImportRow> ReadRows(Stream excelStream);
}
