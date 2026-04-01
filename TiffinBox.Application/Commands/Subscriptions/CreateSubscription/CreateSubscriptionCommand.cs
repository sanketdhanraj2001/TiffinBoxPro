using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;

namespace TiffinBox.Application.Commands.Subscriptions.CreateSubscription
{
    public record CreateSubscriptionCommand(
     Guid CustomerId,
     Guid PlanId,
     DateTime StartDate,
     List<MealSelectionDto> MealSelections) : IRequest<ApiResult<SubscriptionDto>>;

    public record MealSelectionDto(Guid MenuItemId, List<DayOfWeek> DaysOfWeek);

    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, ApiResult<SubscriptionDto>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IPlanRepository _planRepository;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

        public CreateSubscriptionCommandHandler(
            ISubscriptionRepository subscriptionRepository,
            IPlanRepository planRepository,
            IPaymentService paymentService,
            IMapper mapper,
            ILogger<CreateSubscriptionCommandHandler> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
            _paymentService = paymentService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResult<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken ct)
        {
            try
            {
                // Validate plan exists
                var plan = await _planRepository.GetByIdAsync(request.PlanId, ct);
                if (plan == null)
                    return ApiResult<SubscriptionDto>.Fail("Subscription plan not found");

                // Check if customer already has active subscription with this vendor
                var hasActiveSubscription = await _subscriptionRepository.HasActiveSubscriptionAsync(
                    request.CustomerId, plan.VendorId, ct);

                if (hasActiveSubscription)
                    return ApiResult<SubscriptionDto>.Fail("You already have an active subscription with this vendor");

                // Create subscription domain entity
                var subscription = Subscription.Create(
                    request.CustomerId,
                    plan,
                    request.StartDate,
                    request.MealSelections.Select(m => new MealSelection(m.MenuItemId, m.DaysOfWeek)).ToList());

                // Process payment
                var paymentResult = await _paymentService.CreateOrderAsync(
                    subscription.TotalAmount.Amount,
                    subscription.TotalAmount.Currency,
                    subscription.Id.ToString(),
                    ct);

                if (!paymentResult.Success)
                    return ApiResult<SubscriptionDto>.Fail("Payment initiation failed", paymentResult.ErrorMessage);

                // Save subscription
                await _subscriptionRepository.AddAsync(subscription, ct);
                await _subscriptionRepository.SaveChangesAsync(ct);

                var subscriptionDto = _mapper.Map<SubscriptionDto>(subscription);
                subscriptionDto.PaymentOrderId = paymentResult.OrderId;

                _logger.LogInformation("Subscription created successfully: {SubscriptionId} for customer {CustomerId}",
                    subscription.Id, request.CustomerId);

                return ApiResult<SubscriptionDto>.Ok(subscriptionDto, "Subscription created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for customer {CustomerId}", request.CustomerId);
                return ApiResult<SubscriptionDto>.Fail("An error occurred while creating subscription");
            }
        }
    }
}
