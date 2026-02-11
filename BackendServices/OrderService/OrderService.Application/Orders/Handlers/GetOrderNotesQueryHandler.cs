using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Orders.Queries;
using OrderService.Application.Repositories;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class GetOrderNotesQueryHandler : IQueryHandler<GetOrderNotesQuery, IReadOnlyList<OrderNoteDto>>
    {
        private readonly IOrderNoteRepository _noteRepository;
        private readonly IMapper _mapper;

        public GetOrderNotesQueryHandler(IOrderNoteRepository noteRepository, IMapper mapper)
        {
            _noteRepository = noteRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderNoteDto>> Handle(GetOrderNotesQuery query, CancellationToken ct)
        {
            var items = await _noteRepository.GetByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderNoteDto>>(items);
        }
    }
}
