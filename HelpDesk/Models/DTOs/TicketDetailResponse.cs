using System;
using System.Collections.Generic;

namespace HelpDesk.Models.DTOs
{
    /// <summary>
    /// Response model utama yang menggabungkan seluruh data detail sebuah ticket.
    /// </summary>
    public class TicketDetailResponse
    {
        public TicketInfoResponse Ticket { get; set; } = null!;
        public IEnumerable<CommentResponse> Comments { get; set; } = [];
        public SlaResponse? Sla { get; set; }
        public IEnumerable<AttachmentResponse> Attachments { get; set; } = [];
        public IEnumerable<ActivityLogResponse> ActivityLogs { get; set; } = [];
    }

    /// <summary>
    /// Data inti/informasi dasar mengenai ticket.
    /// </summary>
    public class TicketInfoResponse
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? AffectedUser { get; set; }
        public Guid? RelatedTicketId { get; set; }
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public string? Category { get; set; }
        public string? Creator { get; set; }
        public string? Assignee { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    /// <summary>
    /// Data riwayat komentar pada ticket.
    /// </summary>
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? User { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data informasi durasi dan status pemenuhan SLA.
    /// </summary>
    public class SlaResponse
    {
        public DateTime DeadlineAt { get; set; }
        public bool IsBreached { get; set; }
        public DateTime? BreachedAt { get; set; }
    }

    /// <summary>
    /// Data daftar file lampiran yang ada di dalam ticket.
    /// </summary>
    public class AttachmentResponse
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = null!;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Data jejak audit trail/riwayat aktivitas perubahan status ticket.
    /// </summary>
    public class ActivityLogResponse
    {
        public string Action { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}