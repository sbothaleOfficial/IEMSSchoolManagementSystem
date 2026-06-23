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

    /// <summary>
    /// Resolved, display-ready fields for one ID card. Used for students AND employees
    /// (teachers / staff); the front shows whichever optional fields are non-empty.
    /// </summary>
    public record IdCardData
    {
        // Holder name + the primary identifier (label is configurable so the same card
        // reads "Student ID" for students and "Employee ID" for staff/teachers).
        public string StudentName { get; init; } = string.Empty;
        public string IdLabel { get; init; } = "Student ID";
        public string StudentNumber { get; init; } = string.Empty;

        public string FatherName { get; init; } = string.Empty;
        public string Designation { get; init; } = string.Empty;  // employees (e.g. "Teacher", "Cook")
        public string ClassName { get; init; } = string.Empty;     // students
        public string DateOfBirth { get; init; } = string.Empty;
        public string BloodGroup { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;         // employees
        public string ParentMobile { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public byte[]? Photo { get; init; }

        // Pre-rendered photo (rounded corners), built on the UI thread by the caller.
        public byte[]? PhotoRounded { get; init; }
    }

    /// <summary>
    /// Tiles portrait ID cards across A4 pages (logo-blue theme) so a full sheet can be printed
    /// and cut. Works for students and employees (teachers / staff). The card size is selectable
    /// (CR80 default); the layout scales proportionally. The decorative background is supplied as a
    /// pre-rendered PNG and composited behind the vector content. An optional back side (terms,
    /// signature, contacts) follows each front.
    /// </summary>
    public class IdCardDocument : IDocument
    {
        private const float RefHeightMm = 85.6f;

        // Theme colours, matched to the school logo's blue (#0070D0).
        private const string BandBlue = "#0070D0";  // header/footer bands and the T&C pill
        private const string InkBlue = "#0A4C96";   // text & borders on white
        private const string Gold = "#F0B429";
        private const string Serif = "Georgia";     // registered at startup; falls back gracefully

        private readonly IReadOnlyList<IdCardData> _cards;
        private readonly SchoolInfo _school;
        private readonly byte[]? _logo;
        private readonly byte[] _frontBg;
        private readonly byte[]? _backBg;
        private readonly bool _includeBack;
        private readonly IdCardSize _size;
        private readonly float _s;

        public IdCardDocument(IReadOnlyList<IdCardData> cards, SchoolInfo school, byte[]? logo,
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
                    // ---- Header (logo blends onto the matching blue band) ----
                    col.Item().Height(CardH * 0.17f).PaddingHorizontal(5 * s).PaddingTop(3 * s).Row(row =>
                    {
                        if (_logo != null && _logo.Length > 0)
                            row.ConstantItem(28 * s).AlignMiddle().Height(26 * s).Image(_logo).FitArea();
                        row.RelativeItem().PaddingLeft(4 * s).AlignMiddle().Column(c =>
                        {
                            c.Item().Text(_school.Name.ToUpperInvariant())
                                .FontFamily(Serif).FontColor(Colors.White).Bold().FontSize(7 * s).LineHeight(1.05f);
                            if (!string.IsNullOrWhiteSpace(_school.Tagline))
                                c.Item().PaddingTop(1 * s).Text(_school.Tagline)
                                    .FontColor(Gold).FontSize(4.8f * s).LineHeight(1f);
                        });
                    });

                    // ---- Photo ----
                    col.Item().PaddingTop(2 * s).AlignCenter()
                        .Width(25 * s, Unit.Millimetre).Height(30 * s, Unit.Millimetre)
                        .Background(Colors.White).Padding(1.3f * s)
                        .Border(1.3f * s).BorderColor(InkBlue)
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
                    col.Item().PaddingTop(2 * s).AlignCenter().Text(card.StudentName.ToUpperInvariant())
                        .FontFamily(Serif).Bold().FontSize(9 * s).FontColor(InkBlue);
                    col.Item().PaddingTop(1 * s).PaddingHorizontal(22 * s).LineHorizontal(0.6f * s).LineColor(Gold);

                    // ---- Details (only the fields that apply to this holder are shown) ----
                    col.Item().PaddingTop(2 * s).PaddingHorizontal(7 * s).Column(c =>
                    {
                        c.Spacing(1.5f * s);
                        Field(c, card.IdLabel, card.StudentNumber, s);
                        if (!string.IsNullOrWhiteSpace(card.Designation))
                            Field(c, "Designation", card.Designation, s);
                        if (!string.IsNullOrWhiteSpace(card.ClassName))
                            Field(c, "Class", card.ClassName, s);
                        if (!string.IsNullOrWhiteSpace(card.DateOfBirth))
                            Field(c, "Date of Birth", card.DateOfBirth, s);
                        if (!string.IsNullOrWhiteSpace(card.BloodGroup))
                            Field(c, "Blood Group", card.BloodGroup, s);
                        if (!string.IsNullOrWhiteSpace(card.Phone))
                            Field(c, "Phone", card.Phone, s);
                        if (!string.IsNullOrWhiteSpace(card.Address))
                            Field(c, "Address", card.Address, s);
                    });
                });

                // ---- Footer overlay (sits on the blue band at the very bottom) ----
                layers.Layer().AlignBottom().Height(footerH).AlignMiddle().AlignCenter().PaddingHorizontal(6 * s)
                    .Text(FooterText()).FontColor(Colors.White).SemiBold().FontSize(6.5f * s);
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
                    // Header (centred, over the blue band)
                    col.Item().Height(CardH * 0.21f).PaddingHorizontal(6 * s).AlignMiddle().AlignCenter()
                        .Text(_school.Name.ToUpperInvariant())
                        .FontFamily(Serif).FontColor(Colors.White).Bold().FontSize(8.5f * s).LineHeight(1.1f);

                    // Terms & conditions heading (centred pill)
                    col.Item().PaddingTop(3 * s).AlignCenter()
                        .Background(BandBlue).PaddingVertical(2.5f * s).PaddingHorizontal(9 * s)
                        .Text("TERMS & CONDITIONS").FontFamily(Serif).FontColor(Colors.White).Bold().FontSize(7 * s);

                    // Bullets (consistent hanging indent, left-aligned)
                    col.Item().PaddingTop(4 * s).PaddingHorizontal(9 * s).Column(c =>
                    {
                        c.Spacing(3 * s);
                        foreach (var term in _school.Terms)
                            c.Item().Row(r =>
                            {
                                r.ConstantItem(9 * s).PaddingTop(0.5f * s).Text("•").FontColor(BandBlue).Bold().FontSize(8 * s);
                                r.RelativeItem().Text(term).FontSize(6.5f * s).FontColor(Colors.Grey.Darken3).LineHeight(1.2f);
                            });
                    });

                    // Signature pinned to the bottom of the white area (above the footer overlay)
                    col.Item().ExtendVertical().PaddingHorizontal(16 * s).PaddingBottom(4 * s).AlignBottom().Column(c =>
                    {
                        c.Item().AlignCenter().PaddingBottom(1.5f * s).Width(50 * s).LineHorizontal(0.6f * s).LineColor(Colors.Grey.Medium);
                        c.Item().AlignCenter().Text("Principal").FontFamily(Serif).Bold().FontSize(7.5f * s).FontColor(InkBlue);
                    });
                });

                // ---- Footer contacts overlay (left-aligned with aligned labels, on the blue band) ----
                layers.Layer().AlignBottom().Height(footerH).PaddingHorizontal(9 * s).PaddingBottom(5 * s).AlignMiddle().Column(c =>
                {
                    c.Spacing(2.2f * s);
                    if (!string.IsNullOrWhiteSpace(_school.Phone)) Contact(c, "Phone", _school.Phone, s);
                    if (!string.IsNullOrWhiteSpace(_school.Email)) Contact(c, "Email", _school.Email, s);
                    if (!string.IsNullOrWhiteSpace(_school.Address)) Contact(c, "Address", _school.Address, s);
                });
            });
        }

        private static void Field(ColumnDescriptor c, string label, string value, float s)
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(56 * s).Text(label).SemiBold().FontSize(6.8f * s).FontColor(InkBlue);
                row.ConstantItem(6 * s).Text(":").FontSize(6.8f * s).FontColor(InkBlue);
                row.RelativeItem().Text(string.IsNullOrWhiteSpace(value) ? "-" : value).FontSize(6.8f * s).FontColor(Colors.Grey.Darken4);
            });
        }

        private static void Contact(ColumnDescriptor c, string label, string value, float s)
        {
            c.Item().Row(row =>
            {
                row.ConstantItem(42 * s).Text(label).FontColor(Gold).SemiBold().FontSize(6 * s);
                row.RelativeItem().Text(value).FontColor(Colors.White).FontSize(6 * s).LineHeight(1.1f);
            });
        }

        private string FooterText()
        {
            if (!string.IsNullOrWhiteSpace(_school.Website)) return _school.Website;
            if (!string.IsNullOrWhiteSpace(_school.Phone)) return "Tel: " + _school.Phone;
            return _school.Name;
        }
    }
}
