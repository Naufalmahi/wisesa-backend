using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Tracking SLA per ticket.
    /// Menyimpan deadline, status breach, dan waktu breach untuk setiap ticket.
    /// Dibuat otomatis saat ticket di-assign ke teknisi.
    /// </summary>
    public class TicketSla
    {
        /// <summary>Primary key (UUID)</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID ticket yang di-track SLA-nya</summary>
        [Required]
        public Guid TicketId { get; set; }

        /// <summary>ID teknisi yang bertanggung jawab saat SLA dibuat</summary>
        [Required]
        public Guid TechnicianId { get; set; }

        /// <summary>ID SLA Policy yang diterapkan</summary>
        [Required]
        public Guid SlaPolicyId { get; set; }

        /// <summary>Batas waktu deadline penyelesaian</summary>
        [Required]
        public DateTime DeadlineAt { get; set; }

        /// <summary>Waktu saat SLA dilanggar (null jika belum breach)</summary>
        public DateTime? BreachedAt { get; set; }

        /// <summary>Apakah SLA sudah dilanggar</summary>
        public bool IsBreached { get; set; } = false;

        /// <summary>Waktu saat tracking SLA ini mulai dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Ticket terkait</summary>
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }

        /// <summary>Teknisi yang bertanggung jawab</summary>
        [ForeignKey("TechnicianId")]
        public virtual ApplicationUser? Technician { get; set; }

        /// <summary>SLA Policy yang diterapkan</summary>
        [ForeignKey("SlaPolicyId")]
        public virtual SlaPolicy? SlaPolicy { get; set; }
    }
}