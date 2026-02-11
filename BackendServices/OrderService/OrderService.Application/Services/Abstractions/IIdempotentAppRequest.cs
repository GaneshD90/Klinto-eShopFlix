using System.Threading;
using System.Threading.Tasks;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services.Abstractions
{
    public interface IIdempotentAppRequest
    {
        Task<IdempotentRequest?> FindAsync(string key, long? userId, CancellationToken ct = default);
        Task<bool> TryCreateAsync(IdempotentRequest request, CancellationToken ct = default);
        Task PersistResponseAsync(IdempotentRequest request, CancellationToken ct = default);
    }
}
