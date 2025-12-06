namespace OrdersAPI.DTOs
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public int QuantiteStock { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}
