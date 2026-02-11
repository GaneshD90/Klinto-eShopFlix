using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Services.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Persistence.Services
{
    public sealed class IdempotentRequestStore : IIdempotentAppRequest
    {
        private readonly OrderServiceDbContext _db;

        public IdempotentRequestStore(OrderServiceDbContext db)
        {
            _db = db;
        }

        public Task<IdempotentRequest?> FindAsync(string key, long? userId, CancellationToken ct = default)
            => _db.IdempotentRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key && x.UserId == userId, ct);

        public async Task<bool> TryCreateAsync(IdempotentRequest request, CancellationToken ct = default)
        {
            _db.IdempotentRequests.Add(request);
            try
            {
                await _db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                return false;
            }
        }

        public async Task PersistResponseAsync(IdempotentRequest request, CancellationToken ct = default)
        {
            _db.IdempotentRequests.Attach(request);
            _db.Entry(request).Property(p => p.ResponseBody).IsModified = true;
            _db.Entry(request).Property(p => p.StatusCode).IsModified = true;
            _db.Entry(request).Property(p => p.LockedUntil).IsModified = true;
            await _db.SaveChangesAsync(ct);
        }
    }
}
