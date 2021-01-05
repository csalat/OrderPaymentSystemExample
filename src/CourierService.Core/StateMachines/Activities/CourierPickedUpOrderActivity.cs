namespace CourierService.Core.StateMachines.Activities
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using GreenPipes;
    using MassTransit;
    using Sagas;
    using Serilog;
    using Services.Core.Events;

    public class CourierPickedUpOrderActivity :
        Activity<CourierState, OrderPickedUp>
    {
        readonly ConsumeContext _context;

        public CourierPickedUpOrderActivity(ConsumeContext context)
        {
            _context = context;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<CourierState, OrderPickedUp> context,
            Behavior<CourierState, OrderPickedUp> next)
        {
            Log.Information($"Courier State Machine - {nameof(CourierPickedUpOrderActivity)}");
            
            context.Instance.Timestamp = DateTime.Now;
            
            await _context.Send<DeliverOrder>(new
            {
                context.Data.OrderId,
                context.Data.CustomerId,
                context.Data.RestaurantId
            });

            Log.Information($"Sent {nameof(DeliverOrder)}");

            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Faulted<TException>(BehaviorExceptionContext<CourierState, OrderPickedUp, TException> context,
            Behavior<CourierState, OrderPickedUp> next)
            where TException : Exception
        {
            await next.Faulted(context);
        }
    }
}