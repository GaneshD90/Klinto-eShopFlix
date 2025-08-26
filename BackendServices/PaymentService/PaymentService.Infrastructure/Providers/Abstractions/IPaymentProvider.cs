using PaymentService.Application.DTOs;
using Razorpay.Api;


namespace PaymentService.Infrastructure.Providers.Abstractions
{
    public interface IPaymentProvider
    {
        string CreateOrder(RazorPayOrderDTO order);
        Payment GetPaymentDetails(string paymentId);
        string VerifyPayment(PaymentConfirmDTO payment);
    }
}
