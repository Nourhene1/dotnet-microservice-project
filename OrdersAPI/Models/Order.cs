namespace OrdersAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        // 🔥 nouveau champ
        public string PaymentStatus { get; set; } = "Pending";

        public List<OrderItem> Items { get; set; } = new();
    }
}
