
using Engineering_Hub.models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Engineering_Hub.Services
{
    public class CertificatePdfService : ICertificatePdfService
    {
        public byte[] Generate(Certificate certificate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var studentName =
                certificate.Student?.FullName ?? "Student";

            var trackTitle =
                certificate.Track?.Title ?? "Completed Track";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.PageColor(Colors.White);

                    page.Content()
                        .Border(5)
                        .BorderColor("#29456E")
                        .Padding(30)
                        .Column(column =>
                        {
                            column.Spacing(18);

                            column.Item()
                                .AlignCenter()
                                .Text("ENGINEERING HUB")
                                .FontSize(18)
                                .Bold()
                                .FontColor("#29456E");

                            column.Item()
                                .AlignCenter()
                                .Text("CERTIFICATE OF COMPLETION")
                                .FontSize(30)
                                .Bold()
                                .FontColor("#182f57");

                            column.Item()
                                .AlignCenter()
                                .Text("This certificate is presented to")
                                .FontSize(15);

                            column.Item()
                                .AlignCenter()
                                .Text(studentName)
                                .FontSize(27)
                                .Bold()
                                .FontColor("#41a6b1");

                            column.Item()
                                .AlignCenter()
                                .Text("For successfully completing")
                                .FontSize(15);

                            column.Item()
                                .AlignCenter()
                                .Text(trackTitle)
                                .FontSize(22)
                                .Bold();

                            column.Item()
                                .AlignCenter()
                                .Text(
                                    $"Issued on {certificate.IssuedAt:dd MMMM yyyy}")
                                .FontSize(13);

                            column.Item()
                                .AlignCenter()
                                .Text(
                                    $"Certificate ID: {certificate.Id}")
                                .FontSize(11)
                                .FontColor("#555555");
                        });
                });
            }).GeneratePdf();
        }
    }
}
