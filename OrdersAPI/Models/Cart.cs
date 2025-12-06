namespace OrdersAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // Id du client (vient du token JWT, ou du service ClientsAPI)
        public int ClientId { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
