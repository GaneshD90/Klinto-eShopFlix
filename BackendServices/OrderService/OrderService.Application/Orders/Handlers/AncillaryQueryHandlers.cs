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
    public sealed class GetOrderDiscountsQueryHandler : IQueryHandler<GetOrderDiscountsQuery, IReadOnlyList<OrderDiscountDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderDiscountsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderDiscountDto>> Handle(GetOrderDiscountsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetDiscountsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderDiscountDto>>(items);
        }
    }

    public sealed class GetOrderTaxesQueryHandler : IQueryHandler<GetOrderTaxesQuery, IReadOnlyList<OrderTaxDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderTaxesQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderTaxDto>> Handle(GetOrderTaxesQuery query, CancellationToken ct)
        {
            var items = await _repo.GetTaxesByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderTaxDto>>(items);
        }
    }

    public sealed class GetOrderAdjustmentsQueryHandler : IQueryHandler<GetOrderAdjustmentsQuery, IReadOnlyList<OrderAdjustmentDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderAdjustmentsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderAdjustmentDto>> Handle(GetOrderAdjustmentsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetAdjustmentsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderAdjustmentDto>>(items);
        }
    }

    public sealed class GetOrderGiftCardsQueryHandler : IQueryHandler<GetOrderGiftCardsQuery, IReadOnlyList<OrderGiftCardDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderGiftCardsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderGiftCardDto>> Handle(GetOrderGiftCardsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetGiftCardsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderGiftCardDto>>(items);
        }
    }

    public sealed class GetOrderLoyaltyPointsQueryHandler : IQueryHandler<GetOrderLoyaltyPointsQuery, IReadOnlyList<OrderLoyaltyPointDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderLoyaltyPointsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderLoyaltyPointDto>> Handle(GetOrderLoyaltyPointsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetLoyaltyPointsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderLoyaltyPointDto>>(items);
        }
    }

    public sealed class GetOrderDocumentsQueryHandler : IQueryHandler<GetOrderDocumentsQuery, IReadOnlyList<OrderDocumentDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderDocumentsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderDocumentDto>> Handle(GetOrderDocumentsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetDocumentsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderDocumentDto>>(items);
        }
    }

    public sealed class GetOrderMetricQueryHandler : IQueryHandler<GetOrderMetricQuery, OrderMetricDto?>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderMetricQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OrderMetricDto?> Handle(GetOrderMetricQuery query, CancellationToken ct)
        {
            var item = await _repo.GetMetricByOrderIdAsync(query.OrderId, ct);
            return item is null ? null : _mapper.Map<OrderMetricDto>(item);
        }
    }

    public sealed class GetOrderItemOptionsQueryHandler : IQueryHandler<GetOrderItemOptionsQuery, IReadOnlyList<OrderItemOptionDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderItemOptionsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderItemOptionDto>> Handle(GetOrderItemOptionsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetItemOptionsByOrderItemIdAsync(query.OrderItemId, ct);
            return _mapper.Map<IReadOnlyList<OrderItemOptionDto>>(items);
        }
    }

    public sealed class GetOrderFulfillmentAssignmentsQueryHandler : IQueryHandler<GetOrderFulfillmentAssignmentsQuery, IReadOnlyList<OrderFulfillmentAssignmentDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderFulfillmentAssignmentsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderFulfillmentAssignmentDto>> Handle(GetOrderFulfillmentAssignmentsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetFulfillmentAssignmentsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderFulfillmentAssignmentDto>>(items);
        }
    }

    public sealed class GetOrderCancellationsQueryHandler : IQueryHandler<GetOrderCancellationsQuery, IReadOnlyList<OrderCancellationDto>>
    {
        private readonly IOrderAncillaryRepository _repo;
        private readonly IMapper _mapper;

        public GetOrderCancellationsQueryHandler(IOrderAncillaryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<OrderCancellationDto>> Handle(GetOrderCancellationsQuery query, CancellationToken ct)
        {
            var items = await _repo.GetCancellationsByOrderIdAsync(query.OrderId, ct);
            return _mapper.Map<IReadOnlyList<OrderCancellationDto>>(items);
        }
    }
}
