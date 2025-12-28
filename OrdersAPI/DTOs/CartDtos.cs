namespace OrdersAPI.DTOs
{
    public class AddCartItemDto
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; } = string.Empty; // provisoire, on pourra le récupérer d'ArticlesAPI
        public decimal UnitPrice { get; set; }                  // idem
        public int Quantity { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int Quantity { get; set; }
    }
}
