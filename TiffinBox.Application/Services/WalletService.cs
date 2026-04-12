using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Models;
using TiffinBox.Application.DTOs.Customer;
using TiffinBox.Domain.Interfaces;

namespace TiffinBox.Application.Services
{
    public class WalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public WalletService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<WalletDto>> GetMyWalletAsync()
        {
            var userId = _currentUser.GetCurrentUserId();
            var wallet = await _unitOfWork.Wallets.GetByUserIdWithTransactionsAsync(userId, 20);

            if (wallet == null)
                return ApiResponse<WalletDto>.Fail("Wallet not found");

            var dto = new WalletDto
            {
                Balance = wallet.Balance.Amount,
                Currency = wallet.Balance.Currency,
                Transactions = wallet.Transactions.Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    Amount = t.Amount.Amount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return ApiResponse<WalletDto>.Ok(dto);
        }

        public async Task<ApiResponse<string>> AddMoneyAsync(decimal amount, string paymentMethod)
        {
            // Integration with payment gateway would go here
            // For now, directly credit
            var userId = _currentUser.GetCurrentUserId();
            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(userId);

            if (wallet == null)
                return ApiResponse<string>.Fail("Wallet not found");

            await _unitOfWork.BeginTransactionAsync();
            wallet.Credit(amount, "INR", $"Added via {paymentMethod}");
            await _unitOfWork.CompleteAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<string>.Ok(wallet.Id.ToString(), "Money added successfully");
        }
    }
}
