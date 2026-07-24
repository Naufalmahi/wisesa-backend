using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Entity untuk menyimpan informasi file attachment pada ticket.
    /// </summary>
    public class Attachment
    {
        /// <summary>Primary key (UUID)</summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID ticket tempat file ini dilampirkan</summary>
        [Required]
        public Guid TicketId { get; set; }

        /// <summary>ID user yang mengunggah file</summary>
        [Required]
        public Guid UploadedByUserId { get; set; }

        /// <summary>Nama asli file saat diunggah oleh user (Contoh: dokumen_error.png)</summary>
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>Nama unik file yang disimpan di disk/storage untuk menghindari duplikasi (Contoh: att-a1b2c3d4...png)</summary>
        [Required]
        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>Path atau URL lokasi penyimpanan file di server/cloud storage</summary>
        [Required]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Ukuran file dalam satuan bytes</summary>
        public long FileSize { get; set; }

        /// <summary>Tipe konten file (Contoh: image/png, application/pdf)</summary>
        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>Waktu file diunggah</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Ticket terkait</summary>
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }

        /// <summary>User yang mengunggah file</summary>
        [ForeignKey("UploadedByUserId")]
        public virtual ApplicationUser? UploadedBy { get; set; }
    }
}