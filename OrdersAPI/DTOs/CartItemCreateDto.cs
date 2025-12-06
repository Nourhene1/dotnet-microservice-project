namespace OrdersAPI.DTOs
{
    public class CartItemCreateDto
    {
        public int ArticleId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
