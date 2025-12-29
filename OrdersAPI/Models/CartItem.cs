using OrdersAPI.Models;
using System.Text.Json.Serialization;

public class CartItem
{
	public int Id { get; set; }

	public int CartId { get; set; }

	[JsonIgnore]   // ⬅️ LIGNE MAGIQUE
	public Cart? Cart { get; set; }

	public int ArticleId { get; set; }
	public decimal UnitPrice { get; set; }
	public int Quantity { get; set; }
}
