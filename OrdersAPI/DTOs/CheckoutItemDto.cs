namespace OrdersAPI.DTOs
{
    public class CheckoutItemDto
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
