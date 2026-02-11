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
    public sealed class GetReturnManagementQueryHandler : IQueryHandler<GetReturnManagementQuery, IReadOnlyList<ReturnManagementDto>>
    {
        private readonly IOrderReturnRepository _returnRepository;
        private readonly IMapper _mapper;

        public GetReturnManagementQueryHandler(IOrderReturnRepository returnRepository, IMapper mapper)
        {
            _returnRepository = returnRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ReturnManagementDto>> Handle(GetReturnManagementQuery query, CancellationToken ct)
        {
            var items = await _returnRepository.GetReturnManagementByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<ReturnManagementDto>>(items);
        }
    }
}
