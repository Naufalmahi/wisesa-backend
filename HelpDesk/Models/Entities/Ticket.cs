using HelpDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Entity utama ticket helpdesk.
    /// Format nomor ticket: HD-{YYYY}-{NNNNN}
    /// </summary>
    public class Ticket
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor ticket unik otomatis dari server</summary>
        [StringLength(20)]
        public string TicketNumber { get; set; } = string.Empty;

        /// <summary>Judul / ringkasan masalah</summary>
        [Required(ErrorMessage = "Judul ticket wajib diisi")]
        [StringLength(200, ErrorMessage = "Judul maksimal 200 karakter")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Deskripsi lengkap masalah dari user</summary>
        [Required(ErrorMessage = "Deskripsi masalah wajib diisi")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Status ticket saat ini</summary>
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        /// <summary>Tingkat prioritas ticket</summary>
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        /// <summary>Nama user yang terkena dampak kendala</summary>
        [StringLength(150, ErrorMessage = "Affected User maksimal 150 karakter")]
        public string? AffectedUser { get; set; }

        /// <summary>ID Tiket lain yang berhubungan dengan tiket ini</summary>
        public Guid? RelatedTicketId { get; set; }

        /// <summary>ID user pembuat ticket</summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>ID teknisi yang di-assign</summary>
        public Guid? AssignedToId { get; set; }

        /// <summary>ID kategori ticket</summary>
        [Required(ErrorMessage = "Kategori wajib dipilih")]
        public Guid CategoryId { get; set; }

        /// <summary>Tanggal ticket dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Tanggal terakhir ticket diubah</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Tanggal ticket ditutup</summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>User pembuat ticket</summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser? Creator { get; set; }

        /// <summary>Teknisi yang menangani ticket</summary>
        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? Assignee { get; set; }

        /// <summary>Kategori ticket</summary>
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        /// <summary>Daftar komentar pada ticket ini</summary>
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>Daftar SLA tracking untuk ticket ini</summary>
        public virtual ICollection<TicketSla> TicketSlas { get; set; } = new List<TicketSla>();

        /// <summary>Daftar notifikasi terkait ticket ini</summary>
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>Daftar activity log ticket ini</summary>
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

        /// <summary>Daftar escalation untuk ticket ini</summary>
        public virtual ICollection<Escalation> Escalations { get; set; } = new List<Escalation>();

        /// <summary>Daftar file attachment pada ticket ini</summary>
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}