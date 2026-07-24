namespace HelpDesk.Models.Enums
{
    /// <summary>
    /// Tingkat prioritas ticket yang menentukan SLA response & resolve time.
    /// SLA Timer: Low=180m, Medium=120m, High=60m, Critical=30m
    /// </summary>
    public enum TicketPriority
    {
        /// <summary>Prioritas rendah — SLA: 180 menit</summary>
        Low = 0,

        /// <summary>Prioritas sedang — SLA: 120 menit</summary>
        Medium = 1,

        /// <summary>Prioritas tinggi — SLA: 60 menit</summary>
        High = 2,

        /// <summary>Prioritas kritis — SLA: 30 menit</summary>
        Critical = 3
    }
}
