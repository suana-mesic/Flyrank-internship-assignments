using System.Globalization;
using BillingApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace BillingApi.Services;

public sealed class ReportService
{
    static ReportService()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }
    // Renders one tenant's monthly report into a PDF and returns the bytes.
    public byte[] GeneratePdf(ReportData d)
    {
        var inv = CultureInfo.InvariantCulture;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header: title + who/when.
                page.Header().Column(col =>
                {
                    col.Item().Text("Usage & Cost Report").FontSize(20).SemiBold();
                    col.Item().Text($"Tenant: {d.TenantEmail}");
                    col.Item().Text($"Plan: {d.PlanName}   ·   Period: {d.Period}");
                });

                // Content: a small metrics table + the total cost.
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(1); });

                        table.Header(h =>
                        {
                            h.Cell().Text("Metric").SemiBold();
                            h.Cell().AlignRight().Text("Used").SemiBold();
                            h.Cell().AlignRight().Text("Limit").SemiBold();
                        });

                        table.Cell().Text("API calls");
                        table.Cell().AlignRight().Text(d.ApiCallsUsed.ToString("N0", inv));
                        table.Cell().AlignRight().Text(d.ApiCallLimit.ToString("N0", inv));

                        table.Cell().Text("Tokens");
                        table.Cell().AlignRight().Text(d.TokensUsed.ToString("N0", inv));
                        table.Cell().AlignRight().Text(d.TokenLimit.ToString("N0", inv));
                    });

                    col.Item().PaddingTop(15).AlignRight()
                       .Text($"Total cost: ${d.Cost.ToString("0.00000", inv)}").FontSize(13).SemiBold();
                });

                // Footer: generation timestamp + page number.
                page.Footer().AlignRight()
                    .Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });

        return doc.GeneratePdf();
    }
}