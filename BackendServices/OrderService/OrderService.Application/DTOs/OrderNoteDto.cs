using System;

namespace OrderService.Application.DTOs
{
    public class OrderNoteDto
    {
        public Guid OrderNoteId { get; set; }
        public Guid OrderId { get; set; }
        public string NoteType { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsVisibleToCustomer { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
