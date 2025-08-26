using PaymentService.Application.Repositories;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;


namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        PaymentServiceDbContext _db;
        public PaymentRepository(PaymentServiceDbContext db)
        {
            _db = db;
        }
        public bool SavePayementDetails(PaymentDetail model)
        {
            _db.PaymentDetails.Add(model);
           _db.SaveChanges();
            return true;
        }
    }
}
