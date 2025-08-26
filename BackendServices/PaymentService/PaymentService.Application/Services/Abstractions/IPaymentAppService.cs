using PaymentService.Application.DTOs;


namespace PaymentService.Application.Services.Abstractions
{
    public interface IPaymentAppService
    {
        bool SavePaymentDetails(PaymentDetailDTO model);
    }
}
