using ClosedXML.Excel;
using CRM.API.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.API.Services;

public class ExcelService
{
    private readonly AppDbContext _db;
    public ExcelService(AppDbContext db) { _db = db; }

    public async Task<byte[]> ExportServiceRequestsAsync()
    {
        var requests = await _db.ServiceRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Service Requests");

        // Header row
        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Message";
        ws.Cell(1, 5).Value = "Status";
        ws.Cell(1, 6).Value = "Submitted At";
        ws.Cell(1, 7).Value = "Customer ID";

        var headerRow = ws.Range("A1:G1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#6366f1");
        headerRow.Style.Font.FontColor = XLColor.White;

        // Data rows
        for (int i = 0; i < requests.Count; i++)
        {
            var r = requests[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = r.Id;
            ws.Cell(row, 2).Value = r.Name;
            ws.Cell(row, 3).Value = r.Email;
            ws.Cell(row, 4).Value = r.Message ?? "";
            ws.Cell(row, 5).Value = r.Status;
            ws.Cell(row, 6).Value = r.CreatedAt.ToString("dd MMM yyyy HH:mm");
            ws.Cell(row, 7).Value = r.CustomerId.HasValue ? r.CustomerId.Value.ToString() : "";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportSalesRepsAsync()
    {
        var reps = await _db.SalesReps.ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sales Reps");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Name";
        ws.Cell(1, 3).Value = "Email";
        ws.Cell(1, 4).Value = "Phone";
        ws.Cell(1, 5).Value = "City";
        ws.Cell(1, 6).Value = "Expertise";
        ws.Cell(1, 7).Value = "Available";

        var headerRow = ws.Range("A1:G1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#6366f1");
        headerRow.Style.Font.FontColor = XLColor.White;

        for (int i = 0; i < reps.Count; i++)
        {
            var s = reps[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = s.Id;
            ws.Cell(row, 2).Value = s.Name;
            ws.Cell(row, 3).Value = s.Email;
            ws.Cell(row, 4).Value = s.Phone ?? "";
            ws.Cell(row, 5).Value = s.City ?? "";
            ws.Cell(row, 6).Value = s.Expertise ?? "";
            ws.Cell(row, 7).Value = s.IsAvailable ? "Yes" : "No";
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}