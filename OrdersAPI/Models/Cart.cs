namespace OrdersAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public string ClientEmail { get; set; } = string.Empty;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
