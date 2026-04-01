using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken ct);
        Task<PaymentResult> VerifyPaymentAsync(string paymentId, string orderId, string signature, CancellationToken ct);
        Task<RefundResult> RefundAsync(string paymentId, decimal amount, CancellationToken ct);
    }

    public record PaymentResult(bool Success, string? TransactionId, string? OrderId, string? ErrorMessage);
    public record RefundResult(bool Success, string? RefundId, string? ErrorMessage);
}
