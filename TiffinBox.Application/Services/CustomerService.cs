using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Models;
using TiffinBox.Application.DTOs.Customer;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.Interfaces;
using TiffinBox.Domain.Specifications;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Application.Services
{
    public class CustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            INotificationService notificationService,
            ICurrentUserService currentUserService,
            ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedList<VendorListDto>>> GetVendorsAsync(
            string? cuisine,
            string? area,
            int? minRating,
            int page,
            int pageSize)
        {
            var spec = new VendorSpecification
            {
                IsApproved = true,
                IsActive = true,
                Cuisine = cuisine,
                Area = area,
                MinRating = minRating,
                Page = page,
                PageSize = pageSize
            };

            var vendors = await _unitOfWork.Vendors.GetFilteredAsync(spec);
            var totalCount = await _unitOfWork.Vendors.CountFilteredAsync(spec);

            var vendorDtos = vendors.Select(v => new VendorListDto
            {
                Id = v.Id,
                BusinessName = v.BusinessName,
                Description = v.Description,
                LogoUrl = v.LogoUrl,
                Rating = v.Rating,
                TotalRatings = v.TotalRatings,
                Cuisine = v.Cuisine,
                DeliveryFee = v.DeliveryFee?.Amount ?? 0,
                MinOrderAmount = v.MinOrderAmount?.Amount ?? 0,
                EstimatedDeliveryTime = v.EstimatedDeliveryTime
            }).ToList();

            var paginatedList = new PaginatedList<VendorListDto>(
                vendorDtos,
                totalCount,
                page,
                pageSize);

            return ApiResponse<PaginatedList<VendorListDto>>.Ok(paginatedList);
        }

        public async Task<ApiResponse<VendorDetailDto>> GetVendorByIdAsync(Guid vendorId)
        {
            var vendor = await _unitOfWork.Vendors.GetByIdWithDetailsAsync(vendorId);

            if (vendor == null)
                return ApiResponse<VendorDetailDto>.Fail("Vendor not found");

            var vendorDetail = new VendorDetailDto
            {
                Id = vendor.Id,
                BusinessName = vendor.BusinessName,
                Description = vendor.Description,
                LogoUrl = vendor.LogoUrl,
                CoverImageUrl = vendor.CoverImageUrl,
                Rating = vendor.Rating,
                TotalRatings = vendor.TotalRatings,
                Cuisine = vendor.Cuisine,
                Address = vendor.BusinessAddress.ToString(),
                OpeningTime = vendor.OpeningTime,
                ClosingTime = vendor.ClosingTime,
                DeliveryFee = vendor.DeliveryFee?.Amount ?? 0,
                MinOrderAmount = vendor.MinOrderAmount?.Amount ?? 0,
                EstimatedDeliveryTime = vendor.EstimatedDeliveryTime,
                MenuItems = vendor.MenuItems.Where(m => m.IsAvailable).Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price.Amount,
                    Category = m.Category,
                    ImageUrl = m.ImageUrl,
                    IsAvailable = m.IsAvailable,
                    IsVegetarian = m.IsVegetarian
                }).ToList(),
                Plans = vendor.SubscriptionPlans.Where(p => p.IsActive).Select(p => new PlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    PlanType = p.PlanType.ToString(),
                    Price = p.Price.Amount,
                    DurationDays = p.DurationDays,
                    MealsPerDay = p.MealsPerDay
                }).ToList(),
                Reviews = vendor.Reviews.Take(10).Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserName = r.Customer.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return ApiResponse<VendorDetailDto>.Ok(vendorDetail);
        }

        public async Task<ApiResponse<SubscriptionDto>> CreateSubscriptionAsync(CreateSubscriptionDto request)
        {
            var customerId = _currentUserService.GetCurrentUserId();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Get plan
                var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.PlanId);
                if (plan == null)
                    return ApiResponse<SubscriptionDto>.Fail("Subscription plan not found");

                if (!plan.HasCapacity())
                    return ApiResponse<SubscriptionDto>.Fail("Plan is at full capacity");

                // Check existing subscription
                var existingSubscription = await _unitOfWork.Subscriptions
                    .GetActiveSubscriptionAsync(customerId, plan.VendorId);

                if (existingSubscription != null)
                    return ApiResponse<SubscriptionDto>.Fail("You already have an active subscription with this vendor");

                // Validate menu items
                var menuItems = await _unitOfWork.MenuItems
                    .GetByIdsAsync(request.MealSelections.Select(m => m.MenuItemId).ToList());

                if (menuItems.Count != request.MealSelections.Count)
                    return ApiResponse<SubscriptionDto>.Fail("Some menu items are invalid");

                // Create subscription
                var mealSelections = request.MealSelections.Select(m =>
                    new MealSelection(m.MenuItemId, m.DaysOfWeek)).ToList();

                var subscription = Subscription.Create(
                    customerId,
                    plan,
                    request.StartDate,
                    mealSelections);

                await _unitOfWork.Subscriptions.AddAsync(subscription);

                // Process payment
                var paymentResult = await _paymentService.CreateOrderAsync(
                    subscription.TotalAmount.Amount,
                    subscription.TotalAmount.Currency,
                    subscription.Id.ToString());

                if (!paymentResult.Success)
                    return ApiResponse<SubscriptionDto>.Fail("Payment initiation failed", paymentResult.ErrorMessage!);

                // Create payment record
                var payment = Payment.Create(
                    subscription.Id,
                    subscription.TotalAmount,
                    PaymentGateway.Razorpay,
                    paymentResult.OrderId!,
                    PaymentStatus.Pending);

                await _unitOfWork.Payments.AddAsync(payment);

                // Update plan subscriber count
                plan.IncrementSubscribers();

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Send notification
                await _notificationService.SendSubscriptionCreatedAsync(customerId, subscription.Id);

                var dto = new SubscriptionDto
                {
                    Id = subscription.Id,
                    VendorId = plan.VendorId,
                    VendorName = plan.Vendor.BusinessName,
                    PlanName = plan.Name,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    Status = subscription.Status.ToString(),
                    TotalAmount = subscription.TotalAmount.Amount,
                    PaymentOrderId = paymentResult.OrderId
                };

                _logger.LogInformation("Subscription created: {SubscriptionId} for customer {CustomerId}",
                    subscription.Id, customerId);

                return ApiResponse<SubscriptionDto>.Ok(dto, "Subscription created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating subscription for customer {CustomerId}", customerId);
                return ApiResponse<SubscriptionDto>.Fail("An error occurred while creating subscription");
            }
        }

        public async Task<ApiResponse<List<SubscriptionDto>>> GetMySubscriptionsAsync()
        {
            var customerId = _currentUserService.GetCurrentUserId();

            var subscriptions = await _unitOfWork.Subscriptions
                .GetCustomerSubscriptionsAsync(customerId);

            var dtos = subscriptions.Select(s => new SubscriptionDto
            {
                Id = s.Id,
                VendorId = s.VendorId,
                VendorName = s.Plan.Vendor.BusinessName,
                PlanName = s.Plan.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Status = s.Status.ToString(),
                TotalAmount = s.TotalAmount.Amount,
                DeliveredDays = s.DeliveredDays,
                TotalDays = s.TotalDays
            }).ToList();

            return ApiResponse<List<SubscriptionDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<bool>> CancelSubscriptionAsync(Guid subscriptionId, string reason)
        {
            var customerId = _currentUserService.GetCurrentUserId();

            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId);

            if (subscription == null)
                return ApiResponse<bool>.Fail("Subscription not found");

            if (subscription.CustomerId != customerId)
                return ApiResponse<bool>.Fail("You don't have permission to cancel this subscription");

            if (subscription.Status != SubscriptionStatus.Active)
                return ApiResponse<bool>.Fail("Only active subscriptions can be cancelled");

            subscription.Cancel(reason);
            await _unitOfWork.CompleteAsync();

            // Process refund if applicable
            var remainingDays = (subscription.EndDate - DateTime.UtcNow).Days;
            if (remainingDays > 0)
            {
                var refundAmount = (subscription.TotalAmount.Amount / subscription.TotalDays) * remainingDays;
                await _paymentService.RefundAsync(subscriptionId.ToString(), refundAmount);
            }

            await _notificationService.SendSubscriptionCancelledAsync(customerId, subscriptionId);

            return ApiResponse<bool>.Ok(true, "Subscription cancelled successfully");
        }

        public async Task<ApiResponse<PaginatedList<OrderDto>>> GetOrderHistoryAsync(
            OrderStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            var customerId = _currentUserService.GetCurrentUserId();

            var spec = new OrderSpecification
            {
                CustomerId = customerId,
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize,
                IncludeItems = true
            };

            var orders = await _unitOfWork.Orders.GetFilteredAsync(spec);
            var totalCount = await _unitOfWork.Orders.CountFilteredAsync(spec);

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                VendorName = o.Subscription.Plan.Vendor.BusinessName,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount.Amount,
                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    Name = i.MenuItem.Name,
                    Quantity = i.Quantity,
                    Price = i.UnitPrice.Amount
                }).ToList(),
                TrackingUrl = o.TrackingUrl,
                DeliveryAgentName = o.DeliveryAgent?.User.FullName,
                DeliveryAgentPhone = o.DeliveryAgent?.User.PhoneNumber
            }).ToList();

            var paginatedList = new PaginatedList<OrderDto>(
                orderDtos,
                totalCount,
                page,
                pageSize);

            return ApiResponse<PaginatedList<OrderDto>>.Ok(paginatedList);
        }

        public async Task<ApiResponse<OrderDto>> TrackOrderAsync(Guid orderId)
        {
            var customerId = _currentUserService.GetCurrentUserId();

            var order = await _unitOfWork.Orders.GetOrderWithTrackingAsync(orderId);

            if (order == null)
                return ApiResponse<OrderDto>.Fail("Order not found");

            if (order.Subscription.CustomerId != customerId)
                return ApiResponse<OrderDto>.Fail("You don't have permission to track this order");

            var dto = new OrderDto
            {
                Id = order.Id,
                VendorName = order.Subscription.Plan.Vendor.BusinessName,
                DeliveryDate = order.DeliveryDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount.Amount,
                TrackingUrl = order.TrackingUrl,
                DeliveryAgentName = order.DeliveryAgent?.User.FullName,
                DeliveryAgentPhone = order.DeliveryAgent?.User.PhoneNumber,
                DeliveryAgentLocation = order.DeliveryAgent?.CurrentLocation != null
                    ? new LocationDto
                    {
                        Latitude = order.DeliveryAgent.CurrentLocation.Latitude,
                        Longitude = order.DeliveryAgent.CurrentLocation.Longitude
                    }
                    : null,
                EstimatedDeliveryTime = order.OutForDeliveryAt?.AddMinutes(30)
            };

            return ApiResponse<OrderDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> AddReviewAsync(Guid vendorId, int rating, string? comment)
        {
            var customerId = _currentUserService.GetCurrentUserId();

            // Check if customer has ordered from this vendor
            var hasOrdered = await _unitOfWork.Orders
                .HasCustomerOrderedFromVendorAsync(customerId, vendorId);

            if (!hasOrdered)
                return ApiResponse<bool>.Fail("You can only review vendors you have ordered from");

            // Check if already reviewed
            var existingReview = await _unitOfWork.Reviews
                .GetCustomerReviewForVendorAsync(customerId, vendorId);

            if (existingReview != null)
                return ApiResponse<bool>.Fail("You have already reviewed this vendor");

            var review = Review.Create(customerId, vendorId, rating, comment);
            await _unitOfWork.Reviews.AddAsync(review);

            // Update vendor rating
            var vendor = await _unitOfWork.Vendors.GetByIdAsync(vendorId);
            vendor.AddRating(rating);

            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.Ok(true, "Review added successfully");
        }

        public async Task<ApiResponse<WalletDto>> GetWalletBalanceAsync()
        {
            var customerId = _currentUserService.GetCurrentUserId();

            var wallet = await _unitOfWork.Wallets.GetByUserIdAsync(customerId);

            if (wallet == null)
                return ApiResponse<WalletDto>.Fail("Wallet not found");

            var dto = new WalletDto
            {
                Balance = wallet.Balance.Amount,
                Currency = wallet.Balance.Currency,
                Transactions = wallet.Transactions.Take(10).Select(t => new WalletTransactionDto
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

        public async Task<ApiResponse<string>> TopUpWalletAsync(decimal amount, string paymentMethod)
        {
            var customerId = _currentUserService.GetCurrentUserId();

            var paymentResult = await _paymentService.CreateOrderAsync(amount, "INR", $"wallet_topup_{customerId}_{DateTime.UtcNow.Ticks}");

            if (!paymentResult.Success)
                return ApiResponse<string>.Fail("Failed to initiate payment");

            return ApiResponse<string>.Ok(paymentResult.OrderId!, "Payment initiated. Complete the payment to top up wallet.");
        }
    }
}
