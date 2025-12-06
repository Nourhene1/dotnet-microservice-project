namespace OrdersAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
