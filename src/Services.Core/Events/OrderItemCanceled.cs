namespace Services.Core.Events
{
    using System;

    public record OrderItemCanceled
    {
        public OrderItemCanceled()
        {
            Timestamp = DateTime.Now;
        }

        public Guid OrderId { get; init; }
        
        public Guid OrderItemId { get; init; }
        
        public Guid CustomerId { get; init; }
        
        public Guid RestaurantId { get; init; }
        
        public DateTime Timestamp { get; }
    }
}