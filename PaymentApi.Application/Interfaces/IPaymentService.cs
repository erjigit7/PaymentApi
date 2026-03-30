using PaymentApi.Application;

namespace PaymentApi.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> MakePaymentAsync(Guid userId);
}