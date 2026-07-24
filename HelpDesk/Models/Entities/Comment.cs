using HelpDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Komentar pada ticket — bisa berupa pesan publik (terlihat user) 
    /// atau internal note (hanya terlihat teknisi & admin).
    /// Mendukung soft delete via DeletedAt.
    /// </summary>
    public class Comment
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID ticket yang dikomentari</summary>
        [Required]
        public Guid TicketId { get; set; }

        /// <summary>ID user yang menulis komentar</summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>Isi pesan komentar</summary>
        [Required(ErrorMessage = "Pesan komentar wajib diisi")]
        public string Message { get; set; } = string.Empty;

        /// <summary>Tipe komentar: Public (terlihat user) atau InternalNote (hanya internal)</summary>
        public CommentType Type { get; set; } = CommentType.Public;

        /// <summary>Tanggal komentar dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Tanggal komentar dihapus — soft delete (null jika aktif)</summary>
        public DateTime? DeletedAt { get; set; }

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Ticket terkait</summary>
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }

        /// <summary>User yang menulis komentar</summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // ============================================
        // Computed Properties
        // ============================================

        /// <summary>Apakah komentar sudah dihapus (soft delete)</summary>
        [NotMapped]
        public bool IsDeleted => DeletedAt.HasValue;
    }
}
