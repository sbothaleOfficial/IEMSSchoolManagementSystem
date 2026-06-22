using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IEMS.WPF.Pdf
{
    /// <summary>Resolved, display-ready fields for a bonafide certificate.</summary>
    public record BonafideCertificateData
    {
        public string SchoolName { get; init; } = string.Empty;
        public string SchoolAddress { get; init; } = string.Empty;
        public string UDiseCode { get; init; } = string.Empty;
        public string RegNo { get; init; } = string.Empty;
        public byte[]? Logo { get; init; }

        public string StudentNameWithPrefix { get; init; } = string.Empty;
        public string FatherName { get; init; } = string.Empty;
        public string MotherName { get; init; } = string.Empty;
        public string AdmissionDate { get; init; } = string.Empty;
        public string Standard { get; init; } = string.Empty;
        public string StudentNumber { get; init; } = string.Empty;
        public string DateOfBirth { get; init; } = string.Empty;
        public string DateOfBirthInWords { get; init; } = string.Empty;
        public string Caste { get; init; } = string.Empty;
        public string Religion { get; init; } = string.Empty;
        public string PreparedDate { get; init; } = string.Empty;
    }

    /// <summary>A real PDF bonafide certificate, matching the on-screen certificate layout.</summary>
    public class BonafideCertificateDocument : IDocument
    {
        private readonly BonafideCertificateData _d;

        public BonafideCertificateDocument(BonafideCertificateData data)
        {
            _d = data;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(12).FontColor(Colors.Black));

                page.Content().Border(2).BorderColor(Colors.Black).Padding(28).Column(col =>
                {
                    // ---- Header: logo + school identity ----
                    col.Item().Row(row =>
                    {
                        if (_d.Logo != null && _d.Logo.Length > 0)
                            row.ConstantItem(90).Height(90).AlignMiddle().Image(_d.Logo).FitArea();
                        else
                            row.ConstantItem(90);

                        row.RelativeItem().AlignMiddle().Column(c =>
                        {
                            c.Item().AlignCenter().Text(_d.SchoolName.ToUpperInvariant()).Bold().FontSize(20);
                            if (!string.IsNullOrWhiteSpace(_d.SchoolAddress))
                                c.Item().AlignCenter().Text(_d.SchoolAddress).FontSize(11);
                            c.Item().PaddingTop(6).Row(r =>
                            {
                                r.RelativeItem().AlignCenter().Text($"U-Dise Code : {_d.UDiseCode}").FontSize(10);
                                r.RelativeItem().AlignCenter().Text($"School Reg. No. {_d.RegNo}").FontSize(10);
                            });
                        });
                    });

                    col.Item().PaddingVertical(14).LineHorizontal(2).LineColor(Colors.Black);

                    // ---- Title ----
                    col.Item().AlignCenter().Text("BONAFIDE CERTIFICATE")
                        .Bold().FontSize(22).Underline();

                    // ---- Body ----
                    col.Item().PaddingTop(30).PaddingHorizontal(20).Text(t =>
                    {
                        t.AlignCenter();
                        t.DefaultTextStyle(s => s.FontSize(14).LineHeight(1.8f));

                        t.Span("This is to certify that ");
                        t.Span(_d.StudentNameWithPrefix).Bold();
                        t.Span(" S/O, D/O ");
                        t.Span(_d.FatherName).Bold();
                        t.Span(" and Mother ");
                        t.Span(_d.MotherName).Bold();
                        t.Span(" is a bonafide student of this school since ");
                        t.Span(_d.AdmissionDate).Bold();
                        t.Span(" and is now studying in Standard ");
                        t.Span(_d.Standard).Bold();
                        t.Span(". His/Her Reg./Roll No. is ");
                        t.Span(_d.StudentNumber).Bold();
                        t.Span(". The date of birth of this student as per record is ");
                        t.Span(_d.DateOfBirth).Bold();
                        t.Span(" (in words ");
                        t.Span(_d.DateOfBirthInWords).Bold();
                        t.Span("). His/Her caste is ");
                        t.Span(_d.Caste).Bold();
                        t.Span(" and religion ");
                        t.Span(_d.Religion).Bold();
                        t.Span(", according to the School General Register.");
                    });

                    // ---- Footer signatures ----
                    col.Item().PaddingTop(70).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Prepared by,");
                            c.Item().PaddingTop(30).Text(t => { t.Span("Date : ").SemiBold(); t.Span(_d.PreparedDate); });
                        });
                        row.RelativeItem().AlignCenter().AlignBottom().Text("Clerk").Bold();
                        row.RelativeItem().AlignRight().AlignBottom().Text("Principal").Bold();
                    });
                });
            });
        }
    }
}
