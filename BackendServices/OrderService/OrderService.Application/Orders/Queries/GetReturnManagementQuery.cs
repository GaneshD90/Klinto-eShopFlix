using System;
using System.Collections.Generic;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;

namespace OrderService.Application.Orders.Queries
{
    public sealed class GetReturnManagementQuery : IQuery<IReadOnlyList<ReturnManagementDto>>
    {
        public Guid OrderId { get; init; }
    }
}
