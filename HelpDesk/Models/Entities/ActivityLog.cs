using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Log aktivitas pada ticket — mencatat setiap perubahan yang terjadi.
    /// Berfungsi sebagai audit trail untuk tracking siapa melakukan apa dan kapan.
    /// Contoh: perubahan status, reassign teknisi, eskalasi, dll.
    /// </summary>
    public class ActivityLog
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID ticket yang terkait dengan aktivitas</summary>
        [Required]
        public Guid TicketId { get; set; }

        /// <summary>ID user yang melakukan aksi</summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>Deskripsi aksi yang dilakukan (misal: "Status Changed", "Assigned", "Escalated")</summary>
        [Required]
        [StringLength(100, ErrorMessage = "Nama aksi maksimal 100 karakter")]
        public string Action { get; set; } = string.Empty;

        /// <summary>Nilai lama sebelum perubahan (nullable)</summary>
        [StringLength(500)]
        public string? OldValue { get; set; }

        /// <summary>Nilai baru setelah perubahan (nullable)</summary>
        [StringLength(500)]
        public string? NewValue { get; set; }

        /// <summary>Tanggal aktivitas terjadi</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Ticket terkait</summary>
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }

        /// <summary>User yang melakukan aksi</summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
