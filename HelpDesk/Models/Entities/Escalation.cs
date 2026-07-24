using HelpDesk.Models.Enums;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Entity untuk escalation ticket dari teknisi/user.
    /// Track perpindahan assignment dari L1→L2→L3 atau user request escalation.
    /// </summary>
    public class Escalation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TicketId { get; set; }
        public Guid FromUserId { get; set; }    // Teknisi asal / user yg request
        public Guid? ToUserId { get; set; }     // Teknisi tujuan (null jika belum assign)
        public string Reason { get; set; } = string.Empty;
        public EscalationLevel FromLevel { get; set; }
        public EscalationLevel ToLevel { get; set; }
        public EscalationStatus Status { get; set; } = EscalationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Navigation
        public Ticket? Ticket { get; set; }
        public ApplicationUser? FromUser { get; set; }
        public ApplicationUser? ToUser { get; set; }
    }
}
