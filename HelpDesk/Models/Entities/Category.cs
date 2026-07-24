using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Kategori ticket untuk mengelompokkan jenis masalah.
    /// Contoh: Network, Hardware, Software, Security, General.
    /// Digunakan untuk routing otomatis dan laporan.
    /// </summary>
    public class Category
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nama kategori (unik)</summary>
        [Required(ErrorMessage = "Nama kategori wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama kategori maksimal 100 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Deskripsi kategori</summary>
        [StringLength(500, ErrorMessage = "Deskripsi maksimal 500 karakter")]
        public string? Description { get; set; }

        /// <summary>Tanggal kategori dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Daftar ticket dalam kategori ini</summary>
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
