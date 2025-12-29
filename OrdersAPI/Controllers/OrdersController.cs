using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;
using OrdersAPI.DTOs;
using OrdersAPI.Models;
using System.Security.Claims;

namespace OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersDbContext _context;
        private readonly HttpClient _httpClient; // pour récupérer prix ArticlesAPI

        public OrdersController(OrdersDbContext context, IHttpClientFactory httpFactory)
        {
            _context = context;
            _httpClient = httpFactory.CreateClient();
        }

        // ================= CLIENT =================

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyOrders()
        {
            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // ================= ADMIN =================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        // ================= CHECKOUT =================

        [Authorize(Roles = "Client")]
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                return BadRequest("Votre panier est vide.");

            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var order = new Order
            {
                ClientId = clientId,
                OrderDate = DateTime.Now,
                TotalAmount = 0,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = "Pending",
                Items = new List<OrderItem>()
            };

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    return BadRequest($"Quantité invalide pour l'article {item.ArticleId}");

                // ======================
                // 🏷 Récupération du prix ArticlesAPI
                // (optionnel, décommenter si tu veux)
                // ======================
                /*
                var token = Request.Headers["Authorization"].ToString();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

                var priceResponse = await _httpClient.GetAsync(
                    $"https://localhost:7053/gateway/articles/{item.ArticleId}"
                );

                if (!priceResponse.IsSuccessStatusCode)
                    return BadRequest($"Impossible de récupérer le prix de l'article {item.ArticleId}");

                var article = await priceResponse.Content.ReadFromJsonAsync<ArticleDto>();
                decimal realPrice = article.Price;
                */

                // ======================
                // 🚧 version simplifiée : DTO contient déjà le prix
                // ======================
                decimal realPrice = item.UnitPrice;

                order.Items.Add(new OrderItem
                {
                    ArticleId = item.ArticleId,
                    Quantity = item.Quantity,
                    UnitPrice = realPrice
                });

                order.TotalAmount += realPrice * item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Commande validée avec succès ✅",
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.PaymentStatus
            });
        }
    }
}
