namespace CourierService.Core.StateMachines.Activities
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using Sagas;
    using Services.Core.Events;

    public class OrderReadyForPickUpActivity :
        Activity<CourierState, OrderReadyForDelivery>
    {
        readonly ConsumeContext _context;
        readonly ILogger<OrderReadyForPickUpActivity> _logger;

        public OrderReadyForPickUpActivity(ConsumeContext context, ILogger<OrderReadyForPickUpActivity> logger)
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

        public async Task Execute(BehaviorContext<CourierState, OrderReadyForDelivery> context,
            Behavior<CourierState, OrderReadyForDelivery> next)
        {
            _logger.LogInformation($"Courier State Machine - {nameof(OrderReadyForPickUpActivity)} (state = {context.Instance.CurrentState})");
            
            context.Instance.Timestamp = DateTime.Now;

            if (context.Instance.IsOrderReady)
            {
                // TODO: might want to fault if the courier Id has not been set at this point
                if (!context.Instance.CourierId.HasValue)
                    return;

                await _context.Send<PickUpOrder>(
                    new()
                    {
                        CourierId = context.Instance.CourierId.Value,
                        RestaurantId = context.Data.RestaurantId,
                        CustomerId = context.Data.CustomerId,
                        OrderId = context.Data.OrderId
                    });
            
                _logger.LogInformation($"Published - {nameof(PickUpOrder)}");
            }
            else
            {
                // TODO: schedule a wait
            }

            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Faulted<TException>(
            BehaviorExceptionContext<CourierState, OrderReadyForDelivery, TException> context,
            Behavior<CourierState, OrderReadyForDelivery> next)
            where TException : Exception
        {
            await next.Faulted(context);
        }
    }
}