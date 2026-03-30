using PaymentApi.Application;
using PaymentApi.Application.Interfaces;
using PaymentApi.Domain.Entities;

namespace PaymentApi.Application.Services;

public sealed class PaymentService : IPaymentService
{
    private const decimal PaymentAmount = 1.1m;

    private readonly IUserRepository _userRepository;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IUserRepository userRepository, IPaymentRepository paymentRepository)
    {
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentResult> MakePaymentAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
            return PaymentResult.Fail("User not found.", PaymentAmount, 0m);

        if (user.Balance < PaymentAmount)
            return PaymentResult.Fail("Insufficient balance.", PaymentAmount, user.Balance);

        var newBalance = user.Balance - PaymentAmount;

        var updatedUser = new User
        {
            Id = user.Id,
            Login = user.Login,
            PasswordHash = user.PasswordHash,
            Balance = newBalance,
            CreatedAt = user.CreatedAt,
        };

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = PaymentAmount,
            CreatedAt = DateTime.UtcNow,
        };

        await _paymentRepository.AddAsync(payment).ConfigureAwait(false);
        await _userRepository.UpdateAsync(updatedUser).ConfigureAwait(false);

        return PaymentResult.Ok(PaymentAmount, newBalance);
    }
}