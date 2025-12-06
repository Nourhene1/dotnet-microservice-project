using System.Text.Json.Serialization;

namespace OrdersAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }

        [JsonIgnore]
        public Cart Cart { get; set; }   // OK 🚀

        public int ArticleId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
