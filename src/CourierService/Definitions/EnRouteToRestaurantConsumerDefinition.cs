namespace CourierService.Definitions
{
    using Core.Consumers;
    using MassTransit;
    using MassTransit.ConsumeConfigurators;
    using MassTransit.Definition;
    using Services.Core.Configuration;

    public class EnRouteToRestaurantConsumerDefinition :
        ConsumerDefinition<EnRouteToRestaurantConsumer>
    {
        readonly RabbitMqTransportSettings _settings;

        public EnRouteToRestaurantConsumerDefinition(RabbitMqTransportSettings settings)
        {
            _settings = settings;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<EnRouteToRestaurantConsumer> consumerConfigurator)
        {
            consumerConfigurator.UseMessageRetry(r =>
            {
                r.SetRetryPolicy(x => x.Immediate(_settings.MessageRetryImmediatePolicy));
            });
            
            consumerConfigurator.UseScheduledRedelivery(r =>
            {
                r.SetRetryPolicy(x => x.Immediate(_settings.MessageRedeliveryImmediatePolicy));
            });
            
            consumerConfigurator.UseInMemoryOutbox();
        }
    }
}