namespace HelpDesk.Models.Enums
{
    /// <summary>
    /// Status lifecycle sebuah ticket dalam sistem helpdesk.
    /// Alur: Open → InProgress → WaitingUser/Escalated → Resolved → Closed
    /// Ticket yang sudah Closed bisa di-Reopen oleh user.
    /// </summary>
    public enum TicketStatus
    {
        /// <summary>Ticket baru dibuat, belum ditangani teknisi</summary>
        Open = 0,

        /// <summary>Teknisi sedang mengerjakan ticket</summary>
        InProgress = 1,

        /// <summary>Menunggu balasan/informasi dari user, SLA timer pause</summary>
        WaitingUser = 2,

        /// <summary>Ticket dieskalaasi ke tier yang lebih tinggi</summary>
        Escalated = 3,

        /// <summary>Masalah sudah diselesaikan, menunggu konfirmasi user (24 jam auto-close)</summary>
        Resolved = 4,

        /// <summary>Ticket ditutup secara permanen</summary>
        Closed = 5,

        /// <summary>Ticket dibuka kembali oleh user karena masalah belum selesai</summary>
        Reopened = 6
    }
}
