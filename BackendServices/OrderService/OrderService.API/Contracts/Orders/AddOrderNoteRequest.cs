using System;

namespace OrderService.API.Contracts.Orders
{
    public sealed class AddOrderNoteRequest
    {
        public string NoteType { get; set; } = "Internal";
        public string Note { get; set; } = string.Empty;
        public bool IsVisibleToCustomer { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}
