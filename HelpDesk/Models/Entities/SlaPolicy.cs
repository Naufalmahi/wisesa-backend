using HelpDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Kebijakan SLA (Service Level Agreement) berdasarkan tingkat prioritas.
    /// Menentukan batas waktu respons dan resolusi untuk setiap prioritas ticket.
    /// Default: Low=180m, Medium=120m, High=60m, Critical/Urgent=30m
    /// </summary>
    public class SlaPolicy
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nama Kebijakan SLA (Contoh: SLA Prioritas Tinggi)</summary>
        [Required(ErrorMessage = "Nama SLA Policy wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama policy maksimal 100 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Keterangan tambahan mengenai kebijakan SLA ini</summary>
        [StringLength(500, ErrorMessage = "Deskripsi policy maksimal 500 karakter")]
        public string? Description { get; set; }

        /// <summary>Tingkat prioritas yang terkait dengan policy ini</summary>
        [Required(ErrorMessage = "Prioritas wajib ditentukan")]
        public TicketPriority Priority { get; set; }

        /// <summary>Batas waktu respons pertama dalam menit</summary>
        [Required]
        [Range(1, 10080, ErrorMessage = "Response time harus antara 1 - 10080 menit (7 hari)")]
        public int ResponseMinutes { get; set; }

        /// <summary>Batas waktu penyelesaian dalam menit</summary>
        [Required]
        [Range(1, 43200, ErrorMessage = "Resolve time harus antara 1 - 43200 menit (30 hari)")]
        public int ResolveMinutes { get; set; }

        /// <summary>Tanggal policy dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Tanggal policy terakhir diperbarui</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Daftar ticket SLA yang menggunakan policy ini</summary>
        public virtual ICollection<TicketSla> TicketSlas { get; set; } = new List<TicketSla>();
    }
}