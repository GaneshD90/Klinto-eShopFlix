using System;

namespace OrderService.Application.DTOs
{
    public class OrderDocumentDto
    {
        public Guid DocumentId { get; set; }
        public Guid OrderId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string FileFormat { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
