namespace OrderProcessingService.Core.StateMachines.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Automatonymous;
    using Data.Core;
    using GreenPipes;
    using MassTransit;
    using Sagas;
    using Serilog;
    using Service.Grpc.Core;
    using Services.Core.Events;
    using Services.Core.Model;

    public class PrepareOrderRequestedActivity :
        Activity<OrderState, RequestOrderPreparation>
    {
        readonly ConsumeContext _context;
        readonly IGrpcClient<IOrderProcessor> _client;

        public PrepareOrderRequestedActivity(ConsumeContext context, IGrpcClient<IOrderProcessor> client)
        {
            _context = context;
            _client = client;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, RequestOrderPreparation> context,
            Behavior<OrderState, RequestOrderPreparation> next)
        {
            Log.Information($"Order State Machine - {nameof(PrepareOrderRequestedActivity)} (state = {context.Instance.CurrentState})");
            
            context.Instance.Timestamp = DateTime.Now;
            context.Instance.CustomerId = context.Data.CustomerId;
            context.Instance.RestaurantId = context.Data.RestaurantId;
            context.Instance.ExpectedItemCount = context.Data.Items.Length;
            context.Instance.PreparedItemCount = 0;
            context.Instance.CanceledItemCount = 0;
            
            var items = GenerateOrderItemIdentifiers(context.Data.Items).ToList();

            foreach (var item in items)
            {
                await _client.Client.AddExpectedOrderItem(
                    new ()
                    {
                        OrderId = context.Instance.CorrelationId,
                        OrderItemId = item.OrderItemId,
                        Status = item.Status
                    });
            }
            
            await _context.Publish<PrepareOrder>(new
            {
                OrderId = context.Instance.CorrelationId,
                context.Data.CustomerId,
                context.Data.RestaurantId,
                context.Data.AddressId,
                Items = items
            });
            
            Log.Information($"Published - {nameof(PrepareOrder)}");

            await next.Execute(context).ConfigureAwait(false);
        }

        public async Task Faulted<TException>(BehaviorExceptionContext<OrderState, RequestOrderPreparation, TException> context,
            Behavior<OrderState, RequestOrderPreparation> next) where TException : Exception
        {
            await next.Faulted(context);
        }
        
        IEnumerable<Item> GenerateOrderItemIdentifiers(Item[] items) =>
            items.Select(x => new Item
            {
                OrderItemId = NewId.NextGuid(),
                MenuItemId = x.MenuItemId,
                Status = (int)OrderItemStatus.Receipt,
                SpecialInstructions = x.SpecialInstructions
            });
    }
}