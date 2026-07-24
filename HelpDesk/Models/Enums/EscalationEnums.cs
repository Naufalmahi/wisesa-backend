namespace HelpDesk.Models.Enums
{
    /// <summary>Level escalation — dari L1 sampai L3.</summary>
    public enum EscalationLevel
    {
        L1 = 1,
        L2 = 2,
        L3 = 3
    }

    /// <summary>Status escalation request.</summary>
    public enum EscalationStatus
    {
        Pending,    // Menunggu approval/assignment
        Approved,   // Disetujui, ticket dipindahkan
        Rejected,   // Ditolak oleh admin/supervisor
        Cancelled   // Dibatalkan oleh pemohon
    }
}
