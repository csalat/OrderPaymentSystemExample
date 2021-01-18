namespace OrderProcessingService.Core.Consumers
{
    using System.Threading.Tasks;
    using MassTransit;
    using Serilog;
    using Service.Grpc.Core;
    using Services.Core.Events;

    public class PrepareOrderConsumer :
        IConsumer<PrepareOrder>
    {
        readonly IOrderProcessor _client;

        public PrepareOrderConsumer(IGrpcClient<IOrderProcessor> client)
        {
            _client = client.Client;
        }

        public async Task Consume(ConsumeContext<PrepareOrder> context)
        {
            Log.Information($"Consumer - {nameof(PrepareOrderConsumer)} => consumed {nameof(PrepareOrder)} event");
            
            var result = await _client.ProcessOrder(new ()
            {
                OrderId = context.Message.OrderId,
                CustomerId = context.Message.CustomerId,
                RestaurantId = context.Message.RestaurantId,
                AddressId = context.Message.AddressId
            });
            
            if (result.IsSuccessful)
            {
                for (int i = 0; i < context.Message.Items.Length; i++)
                {
                    await context.Publish<PrepareOrderItemRequested>(new
                    {
                        context.Message.OrderId,
                        context.Message.RestaurantId,
                        context.Message.Items[i].OrderItemId,
                        context.Message.Items[i].MenuItemId,
                        context.Message.Items[i].SpecialInstructions
                    });
                }
            }
        }
    }
}