using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> CreateOrderAsync(decimal amount, string currency, string receipt);
        Task<PaymentResult> VerifyPaymentAsync(string paymentId, string orderId, string signature);
        Task<RefundResult> RefundAsync(string paymentId, decimal amount);
        Task<PaymentResult> CapturePaymentAsync(string paymentId, decimal amount);
        Task<PaymentResult> GetPaymentStatusAsync(string paymentId);
    }
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderId { get; set; }
        public string? PaymentId { get; set; }
        public string? Signature { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }

        public PaymentResult() { }

        public PaymentResult(bool success, string? transactionId = null, string? orderId = null, string? errorMessage = null)
        {
            Success = success;
            TransactionId = transactionId;
            OrderId = orderId;
            ErrorMessage = errorMessage;
        }

        public static PaymentResult Ok(string transactionId, string orderId, string? paymentId = null)
        {
            return new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                OrderId = orderId,
                PaymentId = paymentId
            };
        }

        public static PaymentResult Fail(string errorMessage)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class RefundResult
    {
        public bool Success { get; set; }
        public string? RefundId { get; set; }
        public string? PaymentId { get; set; }
        public decimal? Amount { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Status { get; set; }

        public static RefundResult Ok(string refundId, string paymentId, decimal amount)
        {
            return new RefundResult
            {
                Success = true,
                RefundId = refundId,
                PaymentId = paymentId,
                Amount = amount,
                Status = "processed"
            };
        }

        public static RefundResult Fail(string errorMessage)
        {
            return new RefundResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
