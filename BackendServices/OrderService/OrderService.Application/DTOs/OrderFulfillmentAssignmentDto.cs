using System;

namespace OrderService.Application.DTOs
{
    public class OrderFulfillmentAssignmentDto
    {
        public Guid AssignmentId { get; set; }
        public Guid OrderId { get; set; }
        public Guid OrderItemId { get; set; }
        public Guid WarehouseId { get; set; }
        public string AssignmentStatus { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public Guid? PickedBy { get; set; }
        public DateTime? PickedAt { get; set; }
        public Guid? PackedBy { get; set; }
        public DateTime? PackedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
