namespace HelpDesk.Models.Enums
{
    /// <summary>
    /// Tipe notifikasi yang dikirim ke user.
    /// Digunakan untuk menentukan template pesan dan ikon di UI.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>Ticket baru berhasil dibuat</summary>
        TicketCreated = 0,

        /// <summary>Ticket di-assign ke teknisi</summary>
        TicketAssigned = 1,

        /// <summary>Ada perubahan status pada ticket</summary>
        TicketUpdated = 2,

        /// <summary>Ticket sudah diselesaikan oleh teknisi</summary>
        TicketResolved = 3,

        /// <summary>Ticket ditutup secara permanen</summary>
        TicketClosed = 4,

        /// <summary>Ticket dibuka kembali oleh user</summary>
        TicketReopened = 5,

        /// <summary>Ada komentar baru pada ticket</summary>
        NewComment = 6,

        /// <summary>Peringatan SLA mendekati batas waktu (75%/90%)</summary>
        SLAWarning = 7,

        /// <summary>SLA sudah melewati batas waktu (breached)</summary>
        SLABreached = 8
    }
}
