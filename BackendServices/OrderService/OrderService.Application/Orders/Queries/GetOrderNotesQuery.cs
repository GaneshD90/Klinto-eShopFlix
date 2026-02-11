using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetOrderNotesQuery : IQuery<IReadOnlyList<OrderNoteDto>>
    {
        public Guid OrderId { get; init; }
    }
}
