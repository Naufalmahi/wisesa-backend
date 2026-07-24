namespace HelpDesk.Models.Enums
{
    /// <summary>
    /// Role pengguna dalam sistem helpdesk.
    /// Menentukan hak akses dan fitur yang tersedia.
    /// </summary>
    public enum UserRole
    {
        /// <summary>User biasa — bisa membuat ticket dan memantau status</summary>
        User = 0,

        /// <summary>Teknisi — menangani ticket, eskalasi, dan kontribusi Knowledge Base</summary>
        Technician = 1,

        /// <summary>Admin — konfigurasi sistem, manajemen user, dan laporan</summary>
        Admin = 2
    }
}
