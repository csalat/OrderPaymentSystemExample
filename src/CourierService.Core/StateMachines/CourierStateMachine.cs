namespace CourierService.Core.StateMachines
{
    using Activities;
    using Automatonymous;
    using Sagas;
    using Services.Core.Events;

    public class CourierStateMachine :
        MassTransitStateMachine<CourierState>
    {
        public CourierStateMachine()
        {
            InstanceState(x => x.CurrentState, Dispatched, Confirmed, PickedUp, Delivered, Canceled);

            Initially(When(CourierDispatched)
                    .Activity(x => x.OfType<CourierDispatchActivity>())
                    .TransitionTo(Dispatched),
                Ignore(OrderCanceled));

            During(Dispatched,
                When(CourierConfirmed)
                    .Activity(x => x.OfType<CourierConfirmationActivity>())
                    .TransitionTo(Confirmed),
                When(OrderExpired)
                    .Activity(x => x.OfType<OrderExpiredActivity>())
                    .TransitionTo(Canceled),
                When(OrderCanceled)
                    .Activity(x => x.OfType<OrderCanceledActivity>())
                    .TransitionTo(Canceled),
                Ignore(CourierDispatched));

            During(Confirmed,
                When(OrderPickedUp)
                    .Activity(x => x.OfType<OrderPickedUpByActivity>())
                    .TransitionTo(PickedUp),
                When(OrderCanceled)
                    .Activity(x => x.OfType<OrderCanceledActivity>())
                    .TransitionTo(Canceled),
                Ignore(CourierDispatched));

            During(PickedUp,
                When(OrderDelivered)
                    .Activity(x => x.OfType<OrderDeliveryActivity>())
                    .TransitionTo(Delivered),
                Ignore(OrderCanceled),
                Ignore(OrderExpired),
                Ignore(CourierDispatched));

            During(Delivered,
                Ignore(OrderExpired),
                Ignore(OrderCanceled),
                Ignore(CourierDispatched));
            
            During(Canceled,
                Ignore(OrderCanceled),
                Ignore(CourierDispatched),
                Ignore(OrderPickedUp));
            
            Event(() => CourierDispatched,
                x => x.CorrelateById(cxt => cxt.Message.OrderId));
            Event(() => OrderPickedUp,
                x => x.CorrelateById(cxt => cxt.Message.OrderId));
            Event(() => OrderDelivered,
                x => x.CorrelateById(cxt => cxt.Message.OrderId));
            Event(() => CourierConfirmed,
                x => x.CorrelateById(cxt => cxt.Message.OrderId));
            Event(() => OrderCanceled,
                x => x.CorrelateById(cxt => cxt.Message.OrderId));
            Event(() => OrderExpired,
                x => x.CorrelateById(cxt => cxt.Message.OrderId));
        }
        
        public State Dispatched { get; }
        public State Confirmed { get; }
        public State PickedUp { get; }
        public State Delivered { get; }
        public State Canceled { get; }
        
        public Event<CourierDispatched> CourierDispatched { get; }
        public Event<OrderPickedUp> OrderPickedUp { get; }
        public Event<OrderDelivered> OrderDelivered { get; }
        public Event<CourierConfirmed> CourierConfirmed { get; }
        public Event<OrderCanceled> OrderCanceled { get; }
        public Event<OrderExpired> OrderExpired { get; }
    }
}