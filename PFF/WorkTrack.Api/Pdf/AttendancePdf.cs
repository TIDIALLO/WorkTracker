using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WorkTrack.Application;

namespace WorkTrack.Api.Pdf;

public static class AttendancePdf
{
    public static byte[] Create(string titre, string sousTitre, IEnumerable<AttendanceRowDto> rows)
    {
        var doc = Document.Create(c =>
        {
            c.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(titre).FontSize(16);     // pas de Bold ici
                    col.Item().Text(sousTitre).FontSize(10);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(4);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(5);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Matricule");
                        h.Cell().Element(HeaderCell).Text("Nom complet");
                        h.Cell().Element(HeaderCell).Text("Statut");
                        h.Cell().Element(HeaderCell).Text("Min.");
                        h.Cell().Element(HeaderCell).Text("Commentaire");

                        static IContainer HeaderCell(IContainer cc) =>
                            cc.Padding(4).Background(Colors.Grey.Lighten3).BorderBottom(1);
                    });

                    foreach (var r in rows)
                    {
                        table.Cell().Element(Cell).Text(r.Matricule);
                        table.Cell().Element(Cell).Text(r.NomComplet);
                        table.Cell().Element(Cell).Text(r.Statut);
                        table.Cell().Element(Cell).Text(r.MinutesRetard?.ToString() ?? "-");
                        table.Cell().Element(Cell).Text(r.Commentaire ?? "");
                    }

                    static IContainer Cell(IContainer cc) => cc.Padding(4).BorderBottom(0.5f);
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("Généré le ");
                    t.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                    t.Span(" — Page ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }
}
