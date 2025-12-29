using System.ComponentModel.DataAnnotations.Schema;

namespace OrdersAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
		[NotMapped]          // ⬅️ AJOUTER ÇA
		public string? ClientEmail { get; set; }

		public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
