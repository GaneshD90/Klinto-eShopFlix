using PaymentService.Domain.Entities;


namespace PaymentService.Application.Repositories
{
    public interface IPaymentRepository
    {
       bool SavePayementDetails(PaymentDetail model);
    }
}
