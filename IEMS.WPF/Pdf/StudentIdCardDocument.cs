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
        public static readonly IdCardSize CompactCr79 = new("Compact — CR79 (51 × 84 mm)", 51f, 83.9f);
        public static readonly IdCardSize StandardCr80 = new("Standard — CR80 (54 × 86 mm)", 54f, 85.6f);
        public static readonly IdCardSize LargeCr100 = new("Large — CR100 (67 × 99 mm)", 67f, 98.5f);
        public static readonly IdCardSize BadgeA7 = new("Badge — A7 (74 × 105 mm)", 74f, 105f);

        // CR80 is listed first so it is the default selection.
        public static readonly IReadOnlyList<IdCardSize> Presets = new[] { StandardCr80, CompactCr79, LargeCr100, BadgeA7 };
    }

    /// <summary>School details printed on the card (front header + back side).</summary>
    public record SchoolInfo
    {
        public string Name { get; init; } = string.Empty;
        public string Tagline { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Website { get; init; } = string.Empty;
        public IReadOnlyList<string> Terms { get; init; } = new List<string>();
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

        // Pre-rendered assets (built on the UI thread by the caller).
        public byte[]? PhotoRounded { get; init; }
        public byte[]? Barcode { get; init; }
    }

    /// <summary>
    /// Tiles portrait student ID cards across A4 pages (navy/gold theme) so a full sheet can be printed
    /// and cut. The card size is selectable (CR80 default); the layout scales proportionally. The
    /// decorative background and barcode are supplied as pre-rendered PNGs and composited behind/within
    /// the vector content. An optional back side (terms, signature, contacts) follows each front.
    /// </summary>
    public class StudentIdCardDocument : IDocument
    {
        private const float RefHeightMm = 85.6f;
        private static string Navy => Colors.Blue.Darken4;

        private readonly IReadOnlyList<IdCardData> _cards;
        private readonly SchoolInfo _school;
        private readonly byte[]? _logo;
        private readonly byte[] _frontBg;
        private readonly byte[]? _backBg;
        private readonly bool _includeBack;
        private readonly IdCardSize _size;
        private readonly float _s;

        public StudentIdCardDocument(IReadOnlyList<IdCardData> cards, SchoolInfo school, byte[]? logo,
            byte[] frontBg, byte[]? backBg, bool includeBack, IdCardSize? size = null)
        {
            _cards = cards;
            _school = school;
            _logo = logo;
            _frontBg = frontBg;
            _backBg = backBg;
            _includeBack = includeBack && backBg != null;
            _size = size ?? IdCardSize.StandardCr80;
            _s = _size.HeightMm / RefHeightMm;
        }

        // Card height in PDF points (mm × 72/25.4).
        private float CardH => _size.HeightMm * 2.834f;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontColor(Colors.Black));

                page.Content().Inlined(inlined =>
                {
                    inlined.Spacing(6, Unit.Millimetre);
                    inlined.AlignLeft();

                    foreach (var card in _cards)
                    {
                        inlined.Item().Width(_size.WidthMm, Unit.Millimetre).Height(_size.HeightMm, Unit.Millimetre)
                            .Element(c => ComposeFront(c, card));

                        if (_includeBack)
                            inlined.Item().Width(_size.WidthMm, Unit.Millimetre).Height(_size.HeightMm, Unit.Millimetre)
                                .Element(ComposeBack);
                    }
                });
            });
        }

        private void ComposeFront(IContainer container, IdCardData card)
        {
            float s = _s;
            float footerH = CardH * 0.115f;
            container.Layers(layers =>
            {
                layers.Layer().Image(_frontBg).FitUnproportionally();

                // Content (leaves room at the bottom for the footer overlay).
                layers.PrimaryLayer().PaddingBottom(footerH).Column(col =>
                {
                    // ---- Header (over the navy panel) ----
                    col.Item().Height(CardH * 0.185f).PaddingHorizontal(4 * s).PaddingTop(3.5f * s).Row(row =>
                    {
                        if (_logo != null && _logo.Length > 0)
                            row.ConstantItem(32 * s).AlignMiddle().Background(Colors.White).Padding(2 * s)
                                .Height(28 * s).Image(_logo).FitArea();
                        row.RelativeItem().PaddingLeft(4 * s).AlignMiddle().Column(c =>
                        {
                            c.Item().Text(_school.Name.ToUpperInvariant())
                                .FontColor(Colors.White).Bold().FontSize(7.5f * s).LineHeight(1.05f);
                            if (!string.IsNullOrWhiteSpace(_school.Tagline))
                                c.Item().PaddingTop(1 * s).Text(_school.Tagline)
                                    .FontColor(CardArtGold).FontSize(5f * s).LineHeight(1f);
                        });
                    });

                    // ---- Photo ----
                    col.Item().PaddingTop(2.5f * s).AlignCenter()
                        .Width(26 * s, Unit.Millimetre).Height(32 * s, Unit.Millimetre)
                        .Background(Colors.White).Padding(1.3f * s)
                        .Border(1.1f * s).BorderColor(Navy)
                        .Element(box =>
                        {
                            var photo = card.PhotoRounded ?? card.Photo;
                            if (photo != null && photo.Length > 0)
                                box.Image(photo).FitArea();
                            else
                                box.Background(Colors.Grey.Lighten3).AlignMiddle().AlignCenter()
                                   .Text("No Photo").FontSize(6 * s).FontColor(Colors.Grey.Medium);
                        });

                    // ---- Name ----
                    col.Item().PaddingTop(2.5f * s).AlignCenter().Text(card.StudentName.ToUpperInvariant())
                        .Bold().FontSize(9 * s).FontColor(Navy);

                    // ---- Details ----
                    col.Item().PaddingTop(2.5f * s).PaddingHorizontal(7 * s).Column(c =>
                    {
                        c.Spacing(1.8f * s);
                        Field(c, "Student ID", card.StudentNumber, s);
                        Field(c, "Class", card.ClassName, s);
                        Field(c, "Date of Birth", card.DateOfBirth, s);
                        Field(c, "Blood Group", string.IsNullOrWhiteSpace(card.BloodGroup) ? "-" : card.BloodGroup, s);
                    });

                    // ---- Barcode (pushed to the bottom, just above the footer) ----
                    col.Item().ExtendVertical().PaddingHorizontal(9 * s).PaddingTop(2 * s).AlignBottom().AlignCenter()
                        .Element(b =>
                        {
                            if (card.Barcode != null && card.Barcode.Length > 0)
                                b.Height(8.5f * s).Image(card.Barcode).FitHeight();
                        });
                });

                // ---- Footer overlay (sits on the navy band at the very bottom) ----
                layers.Layer().AlignBottom().Height(footerH).AlignMiddle().AlignCenter().PaddingHorizontal(6 * s)
                    .Text(FooterText()).FontColor(Colors.White).FontSize(6.5f * s);
            });
        }

        private void ComposeBack(IContainer container)
        {
            float s = _s;
            float footerH = CardH * 0.21f;
            container.Layers(layers =>
            {
                if (_backBg != null) layers.Layer().Image(_backBg).FitUnproportionally();

                layers.PrimaryLayer().PaddingBottom(footerH).Column(col =>
                {
                    // Header (over navy panel)
                    col.Item().Height(CardH * 0.21f).PaddingHorizontal(6 * s).AlignMiddle()
                        .Text(_school.Name.ToUpperInvariant()).FontColor(Colors.White).Bold().FontSize(8.5f * s).LineHeight(1.05f);

                    // Terms & conditions heading
                    col.Item().PaddingTop(1 * s).PaddingHorizontal(8 * s).AlignCenter()
                        .Background(Navy).PaddingVertical(2 * s).PaddingHorizontal(6 * s)
                        .Text("TERMS & CONDITIONS").FontColor(Colors.White).Bold().FontSize(7 * s);

                    col.Item().PaddingTop(4 * s).PaddingHorizontal(8 * s).Column(c =>
                    {
                        c.Spacing(3 * s);
                        foreach (var term in _school.Terms)
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(8 * s).Text("•").FontColor(Navy).Bold().FontSize(7 * s);
                                r.RelativeItem().Text(term).FontSize(6.5f * s).FontColor(Colors.Grey.Darken3).LineHeight(1.15f);
                            });
                    });

                    // Signature pinned to the bottom of the white area (above the footer overlay)
                    col.Item().ExtendVertical().PaddingHorizontal(14 * s).PaddingBottom(4 * s).AlignBottom().Column(c =>
                    {
                        c.Item().PaddingBottom(1 * s).LineHorizontal(0.6f * s).LineColor(Colors.Grey.Medium);
                        c.Item().AlignCenter().Text("Principal").Bold().FontSize(7 * s).FontColor(Navy);
                    });
                });

                // ---- Footer contacts overlay (on the navy band) ----
                layers.Layer().AlignBottom().Height(footerH).PaddingHorizontal(8 * s).PaddingBottom(4 * s).AlignMiddle().Column(c =>
                {
                    c.Spacing(2f * s);
                    if (!string.IsNullOrWhiteSpace(_school.Phone)) Contact(c, "Tel", _school.Phone, s);
                    if (!string.IsNullOrWhiteSpace(_school.Email)) Contact(c, "Email", _school.Email, s);
                    if (!string.IsNullOrWhiteSpace(_school.Address)) Contact(c, "Addr", _school.Address, s);
                });
            });
        }

        private static void Field(ColumnDescriptor c, string label, string value, float s)
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(58 * s).Text(label).SemiBold().FontSize(6.8f * s).FontColor(Colors.Blue.Darken4);
                row.ConstantItem(6 * s).Text(":").FontSize(6.8f * s).FontColor(Colors.Blue.Darken4);
                row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(6.8f * s).FontColor(Colors.Grey.Darken4);
            });
        }

        private static void Contact(ColumnDescriptor c, string label, string value, float s)
        {
            c.Item().Text(t =>
            {
                t.Span($"{label}: ").FontColor(CardArtGold).SemiBold().FontSize(6 * s);
                t.Span(value).FontColor(Colors.White).FontSize(6 * s);
            });
        }

        private string FooterText()
        {
            if (!string.IsNullOrWhiteSpace(_school.Website)) return _school.Website;
            if (!string.IsNullOrWhiteSpace(_school.Phone)) return "Tel: " + _school.Phone;
            return _school.Name;
        }

        // Gold accent colour matching CardArt.Gold (#F0B429).
        private static string CardArtGold => "#F0B429";
    }
}
