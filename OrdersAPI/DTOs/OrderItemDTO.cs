namespace OrdersAPI.DTOs
{
    public class OrderItemDTO
    {
        public int ArticleId { get; set; }
        public string Nom { get; set; }   // ⭐ nom article
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
