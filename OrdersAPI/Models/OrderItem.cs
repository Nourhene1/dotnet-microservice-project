namespace OrdersAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ArticleId { get; set; }     // vient d'ArticlesAPI
        public string ArticleName { get; set; } = string.Empty; // snapshot
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
