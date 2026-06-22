using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IEMS.WPF.Pdf
{
    /// <summary>Resolved, display-ready fields for one student ID card.</summary>
    public record IdCardData
    {
        public string StudentName { get; init; } = string.Empty;
        public string FatherName { get; init; } = string.Empty;
        public string ClassName { get; init; } = string.Empty;
        public string StudentNumber { get; init; } = string.Empty;
        public string DateOfBirth { get; init; } = string.Empty;
        public string BloodGroup { get; init; } = string.Empty;
        public string ParentMobile { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public byte[]? Photo { get; init; }
    }

    /// <summary>
    /// Tiles standard CR80 vertical (54 x 85.6 mm) student ID cards across A4 pages, so a full sheet
    /// can be printed on photo paper and cut (9 per A4). Content is vector + full-resolution embedded
    /// photos, so print quality is limited only by the source photo, not by rasterisation.
    /// </summary>
    public class StudentIdCardDocument : IDocument
    {
        // CR80 credit-card size, portrait orientation.
        private const float CardWidthMm = 54f;
        private const float CardHeightMm = 85.6f;

        private readonly IReadOnlyList<IdCardData> _cards;
        private readonly string _schoolName;
        private readonly string _schoolAddress;
        private readonly byte[]? _logo;

        public StudentIdCardDocument(IReadOnlyList<IdCardData> cards, string schoolName, string schoolAddress, byte[]? logo)
        {
            _cards = cards;
            _schoolName = schoolName;
            _schoolAddress = schoolAddress;
            _logo = logo;
        }

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontColor(Colors.Black));

                // Inlined flows the fixed-size cards left-to-right, wrapping and paginating automatically.
                page.Content().Inlined(inlined =>
                {
                    inlined.Spacing(6, Unit.Millimetre);
                    inlined.AlignLeft();

                    foreach (var card in _cards)
                    {
                        inlined.Item()
                            .Width(CardWidthMm, Unit.Millimetre)
                            .Height(CardHeightMm, Unit.Millimetre)
                            .Element(c => ComposeCard(c, card));
                    }
                });
            });
        }

        private void ComposeCard(IContainer container, IdCardData card)
        {
            container.Border(0.8f).BorderColor(Colors.Grey.Darken1).Column(col =>
            {
                // ---- Header band: logo + school name ----
                col.Item().Background(Colors.Blue.Darken2).Padding(3).Column(h =>
                {
                    if (_logo != null && _logo.Length > 0)
                        h.Item().AlignCenter().Height(20).Image(_logo).FitHeight();
                    h.Item().AlignCenter().PaddingTop(2).Text(_schoolName)
                        .FontColor(Colors.White).Bold().FontSize(7).LineHeight(1f);
                });

                // ---- Photo (centred passport-style) ----
                col.Item().PaddingTop(5).AlignCenter()
                    .Width(28, Unit.Millimetre).Height(34, Unit.Millimetre)
                    .Border(0.7f).BorderColor(Colors.Grey.Medium).Background(Colors.Grey.Lighten4)
                    .AlignMiddle().AlignCenter().Element(box =>
                    {
                        if (card.Photo != null && card.Photo.Length > 0)
                            box.Image(card.Photo).FitArea();
                        else
                            box.Text("No\nPhoto").FontSize(6).FontColor(Colors.Grey.Medium).AlignCenter();
                    });

                // ---- Name ----
                col.Item().PaddingTop(4).PaddingHorizontal(3).AlignCenter()
                    .Text(card.StudentName).Bold().FontSize(8.5f).FontColor(Colors.Blue.Darken2);

                col.Item().PaddingHorizontal(5).PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);

                // ---- Details ----
                col.Item().PaddingTop(2).PaddingHorizontal(5).Column(c =>
                {
                    c.Spacing(1.5f);
                    Field(c, "Class", card.ClassName);
                    Field(c, "Roll No", card.StudentNumber);
                    Field(c, "DOB", card.DateOfBirth);
                    Field(c, "Blood Group", string.IsNullOrWhiteSpace(card.BloodGroup) ? "-" : card.BloodGroup);
                    Field(c, "Father", card.FatherName);
                    Field(c, "Mobile", card.ParentMobile);
                });
            });
        }

        private static void Field(ColumnDescriptor c, string label, string value)
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(48).Text($"{label}").FontSize(6.5f).SemiBold().FontColor(Colors.Grey.Darken2);
                row.ConstantItem(5).Text(":").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(6.5f);
            });
        }
    }
}
