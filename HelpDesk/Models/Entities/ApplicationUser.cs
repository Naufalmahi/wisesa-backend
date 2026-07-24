using Microsoft.AspNetCore.Identity;
using HelpDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.Entities
{
    /// <summary>
    /// Entity user yang meng-extend IdentityUser dengan Guid sebagai primary key.
    /// Menyimpan data tambahan seperti nama lengkap, role, dan status aktif.
    /// Terhubung ke: Tickets (sebagai pembuat & assignee), Comments, Notifications, ActivityLogs
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>Nama lengkap pengguna</summary>
        [Required(ErrorMessage = "Nama wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Role pengguna: User, Technician, atau Admin</summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>Status aktif akun (false = nonaktif/diblokir)</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Tanggal akun dibuat</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Tanggal terakhir data akun diubah</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ============================================
        // Navigation Properties
        // ============================================

        /// <summary>Daftar ticket yang dibuat oleh user ini</summary>
        public virtual ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        /// <summary>Daftar ticket yang di-assign ke user ini (teknisi)</summary>
        public virtual ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        /// <summary>Daftar komentar yang ditulis user ini</summary>
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        /// <summary>Daftar notifikasi untuk user ini</summary>
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        /// <summary>Daftar activity log yang dilakukan user ini</summary>
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

        /// <summary>Daftar SLA ticket yang terkait user ini</summary>
        public virtual ICollection<TicketSla> TicketSlas { get; set; } = new List<TicketSla>();
    }
}
