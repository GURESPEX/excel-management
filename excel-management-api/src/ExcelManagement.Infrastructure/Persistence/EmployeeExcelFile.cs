using ClosedXML.Excel;
using ExcelManagement.Application.Employees;

namespace ExcelManagement.Infrastructure.Persistence;

public class EmployeeExcelFile : IEmployeeExcelFile
{
    private static readonly string[] Headers = ["Id", "Name", "Department", "Salary", "Join Date", "Status"];

    public byte[] WriteWorkbook(IReadOnlyList<EmployeeListItemDto> employees)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Employees");

        for (var col = 0; col < Headers.Length; col++)
        {
            sheet.Cell(1, col + 1).Value = Headers[col];
        }

        var row = 2;
        foreach (var e in employees)
        {
            sheet.Cell(row, 1).Value = e.Id;
            sheet.Cell(row, 2).Value = e.Name;
            sheet.Cell(row, 3).Value = e.DepartmentName;
            sheet.Cell(row, 4).Value = e.Salary;
            sheet.Cell(row, 5).Value = e.JoinDate.ToString(EmployeeValidator.JoinDateFormat);
            sheet.Cell(row, 6).Value = e.IsActive ? "Active" : "In Active";
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public IReadOnlyList<EmployeeImportRow> ReadRows(Stream excelStream)
    {
        using var workbook = new XLWorkbook(excelStream);
        var sheet = workbook.Worksheets.First();

        var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in sheet.Row(1).CellsUsed())
        {
            columnIndex[cell.GetString().Trim()] = cell.Address.ColumnNumber;
        }

        int Col(string name) => columnIndex.GetValueOrDefault(name, -1);
        string Cell(IXLRow xlRow, int col) => col < 0 ? string.Empty : xlRow.Cell(col).GetString().Trim();

        var nameCol = Col("Name");
        var deptCol = Col("Department");
        var salaryCol = Col("Salary");
        var joinDateCol = Col("Join Date");
        var statusCol = Col("Status");

        var rows = new List<EmployeeImportRow>();
        var lastRowNumber = sheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var r = 2; r <= lastRowNumber; r++)
        {
            var xlRow = sheet.Row(r);
            if (xlRow.IsEmpty())
            {
                continue;
            }

            rows.Add(new EmployeeImportRow(
                r,
                Cell(xlRow, nameCol),
                Cell(xlRow, deptCol),
                Cell(xlRow, salaryCol),
                Cell(xlRow, joinDateCol),
                Cell(xlRow, statusCol)));
        }

        return rows;
    }
}
