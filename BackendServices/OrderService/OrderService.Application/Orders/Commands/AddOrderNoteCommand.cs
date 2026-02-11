using System;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Commands
{
    public sealed class AddOrderNoteCommand : ICommand<OrderNoteDto>
    {
        public Guid OrderId { get; init; }
        public string NoteType { get; init; } = "Internal";
        public string Note { get; init; } = string.Empty;
        public bool IsVisibleToCustomer { get; init; }
        public Guid? CreatedBy { get; init; }
    }
}
