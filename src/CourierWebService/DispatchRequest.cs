namespace CourierWebService
{
    using System;

    public record DispatchRequest
    {
        public Guid OrderId { get; init; }
        
        public Guid CustomerId { get; init; }
        
        public Guid RestaurantId { get; init; }
    }
}