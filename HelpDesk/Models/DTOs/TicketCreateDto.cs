using HelpDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models.DTOs
{
    public class TicketCreateDto
    {
        [Required(ErrorMessage = "Judul wajib diisi")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deskripsi wajib diisi")]
        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public string? AffectedUser { get; set; }

        public Guid? RelatedTicketId { get; set; }

        [Required(ErrorMessage = "Kategori wajib dipilih")]
        public Guid CategoryId { get; set; }
    }
}