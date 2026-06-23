using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IEMS.WPF.Pdf
{
    /// <summary>A standard ID-card size (portrait), in millimetres.</summary>
    public record IdCardSize(string DisplayName, float WidthMm, float HeightMm)
    {
        public override string ToString() => DisplayName;

        // Standard plastic-card sizes (portrait). CR80 is the universal default.
        public static readonly IdCardSize StandardCr80 = new("Standard — CR80 (54 × 86 mm)", 54f, 85.6f);
        public static readonly IdCardSize LargeCr100 = new("Large — CR100 (67 × 99 mm)", 67f, 98.5f);
        public static readonly IdCardSize CompactCr79 = new("Compact — CR79 (51 × 84 mm)", 51f, 83.9f);

        public static readonly IReadOnlyList<IdCardSize> Presets = new[] { StandardCr80, LargeCr100, CompactCr79 };
    }

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
    /// Tiles portrait student ID cards across A4 pages, so a full sheet can be printed on photo paper
    /// and cut. The card size is selectable (CR80 default); the whole layout scales proportionally.
    /// Content is vector + embedded photos, so print quality is limited only by the source photo.
    /// </summary>
    public class StudentIdCardDocument : IDocument
    {
        // Reference size the base layout was tuned at (CR80 portrait). Other sizes scale from this.
        private const float RefHeightMm = 85.6f;

        // Passport aspect for the photo box (35:45). Photos are cropped to match so they fill it.
        public const float PhotoAspectW = 35f;
        public const float PhotoAspectH = 45f;

        private readonly IReadOnlyList<IdCardData> _cards;
        private readonly string _schoolName;
        private readonly string _schoolAddress;
        private readonly byte[]? _logo;
        private readonly IdCardSize _size;
        private readonly float _s; // scale factor relative to the reference CR80 layout

        public StudentIdCardDocument(IReadOnlyList<IdCardData> cards, string schoolName, string schoolAddress,
            byte[]? logo, IdCardSize? size = null)
        {
            _cards = cards;
            _schoolName = schoolName;
            _schoolAddress = schoolAddress;
            _logo = logo;
            _size = size ?? IdCardSize.StandardCr80;
            _s = _size.HeightMm / RefHeightMm;
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
                            .Width(_size.WidthMm, Unit.Millimetre)
                            .Height(_size.HeightMm, Unit.Millimetre)
                            .Element(c => ComposeCard(c, card));
                    }
                });
            });
        }

        private void ComposeCard(IContainer container, IdCardData card)
        {
            float s = _s;
            container.Border(0.8f * s).BorderColor(Colors.Grey.Darken1).Column(col =>
            {
                // ---- Header band: logo + school name ----
                col.Item().Background(Colors.Blue.Darken2).Padding(3 * s).Column(h =>
                {
                    if (_logo != null && _logo.Length > 0)
                        h.Item().AlignCenter().Height(20 * s).Image(_logo).FitHeight();
                    h.Item().AlignCenter().PaddingTop(2 * s).Text(_schoolName)
                        .FontColor(Colors.White).Bold().FontSize(7 * s).LineHeight(1f);
                });

                // ---- Photo (centred passport-style; cropped to 35:45 so it fills) ----
                col.Item().PaddingTop(5 * s).AlignCenter()
                    .Width(28f * s, Unit.Millimetre).Height(36f * s, Unit.Millimetre)
                    .Border(0.7f * s).BorderColor(Colors.Grey.Medium).Background(Colors.Grey.Lighten4)
                    .AlignMiddle().AlignCenter().Element(box =>
                    {
                        if (card.Photo != null && card.Photo.Length > 0)
                            box.Image(card.Photo).FitArea();
                        else
                            box.Text("No\nPhoto").FontSize(6 * s).FontColor(Colors.Grey.Medium).AlignCenter();
                    });

                // ---- Name ----
                col.Item().PaddingTop(4 * s).PaddingHorizontal(3 * s).AlignCenter()
                    .Text(card.StudentName).Bold().FontSize(8.5f * s).FontColor(Colors.Blue.Darken2);

                col.Item().PaddingHorizontal(5 * s).PaddingTop(2 * s).LineHorizontal(0.5f * s).LineColor(Colors.Grey.Lighten1);

                // ---- Details ----
                col.Item().PaddingTop(2 * s).PaddingHorizontal(5 * s).Column(c =>
                {
                    c.Spacing(1.5f * s);
                    Field(c, "Class", card.ClassName, s);
                    Field(c, "Roll No", card.StudentNumber, s);
                    Field(c, "DOB", card.DateOfBirth, s);
                    Field(c, "Blood Group", string.IsNullOrWhiteSpace(card.BloodGroup) ? "-" : card.BloodGroup, s);
                    Field(c, "Father", card.FatherName, s);
                    Field(c, "Mobile", card.ParentMobile, s);
                });
            });
        }

        private static void Field(ColumnDescriptor c, string label, string value, float s)
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(48 * s).Text($"{label}").FontSize(6.5f * s).SemiBold().FontColor(Colors.Grey.Darken2);
                row.ConstantItem(5 * s).Text(":").FontSize(6.5f * s).FontColor(Colors.Grey.Darken2);
                row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(6.5f * s);
            });
        }
    }
}
