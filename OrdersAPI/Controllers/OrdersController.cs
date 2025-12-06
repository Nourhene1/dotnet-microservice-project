using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;
using OrdersAPI.Models;
using OrdersAPI.DTOs;
using System.Security.Claims;

namespace OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _context;
        private readonly HttpClient _http;

        // 👉 URL service Articles
        private const string ARTICLES_URL = "https://localhost:7123/api/Article";

        public OrdersController(
            OrdersDbContext context,
            IHttpClientFactory httpFactory
        )
        {
            _context = context;
            _http = httpFactory.CreateClient();
        }

        // ⭐ CREER UNE COMMANDE À PARTIR DU PANIER (client connecté)
        [HttpPost("me/create-from-cart")]
        public async Task<ActionResult<Order>> CreateOrderFromCart()
        {
            // ➤ Identifier le client
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("Utilisateur non identifié.");

            int clientId = int.Parse(userIdString);

            // ➤ Charger panier
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null || !cart.Items.Any())
                return BadRequest("Panier vide.");

            // ============================================
            // ⭐ 1) Vérification du stock côté ArticlesAPI
            // ============================================
            foreach (var cartItem in cart.Items)
            {
                string urlCheck = $"{ARTICLES_URL}/{cartItem.ArticleId}";
                var resp = await _http.GetAsync(urlCheck);

                if (!resp.IsSuccessStatusCode)
                    return BadRequest($"Article ID={cartItem.ArticleId} introuvable.");

                var article = await resp.Content.ReadFromJsonAsync<ArticleDTO>();

                if (article == null)
                    return BadRequest("Article non lisible depuis API.");

                if (article.QuantiteStock < cartItem.Quantity)
                    return BadRequest(
                        $"❌ Stock insuffisant pour '{article.Nom}'. " +
                        $"Stock restant : {article.QuantiteStock}"
                    );
            }

            // ============================================
            // ⭐ 2) Création de la commande locale
            // ============================================
            var order = new Order
            {
                ClientId = clientId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Items.Sum(i => i.UnitPrice * i.Quantity),
                Items = cart.Items.Select(i => new OrderItem
                {
                    ArticleId = i.ArticleId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);

            // ============================================
            // ⭐ 3) Diminuer le stock dans ArticlesAPI
            // ============================================
            foreach (var cartItem in cart.Items)
            {
                string urlDecrease = $"{ARTICLES_URL}/{cartItem.ArticleId}/decrease-stock";
                await _http.PutAsJsonAsync(urlDecrease, cartItem.Quantity);
            }

            // ============================================
            // ⭐ 4) Vider le panier local
            // ============================================
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // ⭐ COMMANDES DE L’UTILISATEUR
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int clientId = int.Parse(userIdString);

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }
    }
}
