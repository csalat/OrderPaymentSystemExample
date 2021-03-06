namespace OrderProcessingService.Core.StateMachines.Activities
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using Sagas;
    using Services.Core.Events;

    public class VoidOrderItemRequestActivity :
        Activity<OrderItemState, VoidOrderItemRequest>
    {
        readonly ConsumeContext _context;
        readonly ILogger<VoidOrderItemRequestActivity> _logger;

        public VoidOrderItemRequestActivity(ConsumeContext context, ILogger<VoidOrderItemRequestActivity> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderItemState, VoidOrderItemRequest> context,
            Behavior<OrderItemState, VoidOrderItemRequest> next)
        {
            _logger.LogInformation($"Order Item State Machine - {nameof(VoidOrderItemRequestActivity)} (state = {context.Instance.CurrentState})");

            context.Instance.Timestamp = DateTime.Now;

            await _context.Publish<VoidOrderItem>(
                new ()
                {
                    OrderId = context.Data.OrderId,
                    OrderItemId = context.Data.OrderItemId,
                    CustomerId = context.Data.CustomerId,
                    RestaurantId = context.Data.RestaurantId
                });
            
            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<OrderItemState, VoidOrderItemRequest, TException> context,
            Behavior<OrderItemState, VoidOrderItemRequest> next)
            where TException : Exception
        {
            await next.Faulted(context);
        }
    }
}