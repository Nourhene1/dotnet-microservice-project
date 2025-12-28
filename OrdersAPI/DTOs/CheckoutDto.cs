using System.Collections.Generic;

namespace OrdersAPI.DTOs
{
    public class CheckoutDto
    {
        public string PaymentMethod { get; set; } = "Cash";
        public List<CheckoutItemDto> Items { get; set; } = new();

    }
}
