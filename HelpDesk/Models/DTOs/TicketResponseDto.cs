using HelpDesk.Models.Enums;

namespace HelpDesk.Models.DTOs
{
    public class TicketResponseDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? AffectedUser { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}