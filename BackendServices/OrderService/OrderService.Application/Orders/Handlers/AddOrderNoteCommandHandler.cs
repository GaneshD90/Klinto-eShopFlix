using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class AddOrderNoteCommandHandler : ICommandHandler<AddOrderNoteCommand, OrderNoteDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderNoteRepository _noteRepository;

        public AddOrderNoteCommandHandler(
            IOrderRepository orderRepository,
            IOrderNoteRepository noteRepository)
        {
            _orderRepository = orderRepository;
            _noteRepository = noteRepository;
        }

        public async Task<OrderNoteDto> Handle(AddOrderNoteCommand command, CancellationToken ct)
        {
            var order = await _orderRepository.GetByIdAsync(command.OrderId, ct);
            if (order is null)
            {
                throw AppException.NotFound("Order", $"Order {command.OrderId} not found.");
            }

            var now = DateTime.UtcNow;

            var note = new OrderNote
            {
                OrderNoteId = Guid.NewGuid(),
                OrderId = command.OrderId,
                NoteType = command.NoteType,
                Note = command.Note,
                IsVisibleToCustomer = command.IsVisibleToCustomer,
                CreatedBy = command.CreatedBy,
                CreatedAt = now
            };

            await _noteRepository.AddAsync(note, ct);

            return new OrderNoteDto
            {
                OrderNoteId = note.OrderNoteId,
                OrderId = note.OrderId,
                NoteType = note.NoteType,
                Note = note.Note,
                IsVisibleToCustomer = note.IsVisibleToCustomer,
                CreatedBy = note.CreatedBy,
                CreatedAt = note.CreatedAt
            };
        }
    }
}
