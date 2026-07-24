namespace HelpDesk.Models.Enums
{
    /// <summary>
    /// Tipe komentar pada ticket.
    /// Public terlihat oleh user, InternalNote hanya terlihat oleh teknisi & admin.
    /// </summary>
    public enum CommentType
    {
        /// <summary>Komentar publik — terlihat oleh semua pihak termasuk user</summary>
        Public = 0,

        /// <summary>Catatan internal — hanya terlihat oleh teknisi dan admin</summary>
        InternalNote = 1
    }
}
