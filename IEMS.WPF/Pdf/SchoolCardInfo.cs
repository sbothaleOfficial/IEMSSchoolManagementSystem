namespace IEMS.WPF.Pdf
{
    /// <summary>
    /// The school details printed on ID cards (front header + optional back side).
    /// Shared by the student and staff/teacher ID-card flows so they stay in sync.
    /// </summary>
    public static class SchoolCardInfo
    {
        public static SchoolInfo Default => new()
        {
            Name = "Inspire English Medium School, Mardi",
            Tagline = "Excellence in Education • Inspiring Future Leaders",
            Address = "Tah. Maregaon, Dist. Yavatmal (MH) – 445303",
            Phone = "8483949981",
            Email = "inspiremardi@gmail.com",
            Website = "",
            Terms = new[]
            {
                "This ID card is the property of the school.",
                "This card is non-transferable.",
                "If found, please return it to the school office.",
                "The holder must carry this card in school every day."
            }
        };
    }
}
