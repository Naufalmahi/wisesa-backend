using HelpDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Notifikasi untuk user — disimpan di database (tanpa SMTP).
    /// Setiap event penting pada ticket akan membuat record notifikasi baru.
    /// </summary>
    public class Notification
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID user penerima notifikasi</summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>ID ticket terkait (nullable untuk notifikasi sistem umum)</summary>
        public Guid? TicketId { get; set; }

        /// <summary>Tipe notifikasi untuk menentukan template dan ikon</summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>Isi pesan notifikasi</summary>
        [Required]
        [StringLength(500, ErrorMessage = "Pesan notifikasi maksimal 500 karakter")]
        public string Message { get; set; } = string.Empty;

        /// <summary>Status sudah dibaca atau belum</summary>
        public bool IsRead { get; set; } = false;

        /// <summary>Tanggal notifikasi dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>User penerima notifikasi</summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        /// <summary>Ticket terkait notifikasi</summary>
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }
    }
}
