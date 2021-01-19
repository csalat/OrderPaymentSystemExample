namespace OrderProcessingService.Core.StateMachines.Sagas
{
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ExpectedOrderItemMap :
        SagaClassMap<ExpectedOrderItem>
    {
        protected override void Configure(EntityTypeBuilder<ExpectedOrderItem> entity, ModelBuilder model)
        {
            entity.Property(x => x.OrderItemId).IsRequired();
            entity.Property(x => x.OrderId).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.Timestamp).IsRequired();
        }
    }
}