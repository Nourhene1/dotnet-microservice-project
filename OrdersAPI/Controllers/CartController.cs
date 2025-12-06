using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;
using OrdersAPI.DTOs;
using OrdersAPI.Models;
using System.Security.Claims;
using System.Net.Http.Json;

namespace OrdersAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly OrdersDbContext _context;
        private readonly HttpClient _http;

        // URL du service Articles
        private const string ARTICLES_URL = "https://localhost:7123/api/Article";

        public CartController(OrdersDbContext context, IHttpClientFactory httpFactory)
        {
            _context = context;
            _http = httpFactory.CreateClient();
        }

        // ======================================================
        // 🛒 GET PANIER DU CLIENT CONNECTÉ
        // ======================================================
        [HttpGet("me")]
        public async Task<ActionResult<Cart>> GetMyCart()
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int clientId = int.Parse(userIdString);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null)
            {
                cart = new Cart
                {
                    ClientId = clientId,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return Ok(cart);
        }

        // ======================================================
        // 🛒 AJOUT ARTICLE AU PANIER
        // ======================================================
        [HttpPost("me/add")]
        public async Task<ActionResult> AddItem([FromBody] CartItemCreateDto dto)
        {
            string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int clientId = int.Parse(userIdString);

            // 👉 Charger panier
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null)
            {
                cart = new Cart { ClientId = clientId };
                _context.Carts.Add(cart);
            }

            // 👉 Vérifier stock réel côté ArticlesAPI
            var resp = await _http.GetAsync($"{ARTICLES_URL}/{dto.ArticleId}");
            if (!resp.IsSuccessStatusCode)
                return BadRequest("Article introuvable côté stock");

            var article = await resp.Content.ReadFromJsonAsync<ArticleDTO>();
            if (article == null)
                return BadRequest("Impossible de lire l'article");

            // 👉 Vérifier capacité
            if (dto.Quantity > article.QuantiteStock)
                return BadRequest($"Stock insuffisant ({article.QuantiteStock})");

            var existingItem = cart.Items.FirstOrDefault(i => i.ArticleId == dto.ArticleId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + dto.Quantity > article.QuantiteStock)
                    return BadRequest($"Stock insuffisant pour augmenter (Stock dispo = {article.QuantiteStock})");

                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ArticleId = dto.ArticleId,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                });
            }

            await _context.SaveChangesAsync();
            return Ok("Ajout au panier réussi !");
        }

        // ======================================================
        // ➕ AUGMENTER QUANTITÉ
        // ======================================================
        [HttpPut("inc/{articleId}")]
        public async Task<IActionResult> IncreaseItem(int articleId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int clientId = int.Parse(userIdString);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null)
                return NotFound("Panier introuvable");

            var item = cart.Items.FirstOrDefault(i => i.ArticleId == articleId);
            if (item == null)
                return NotFound("Article introuvable dans panier");

            // 👉 Vérifier le stock API avant d'augmenter
            var resp = await _http.GetAsync($"{ARTICLES_URL}/{articleId}");
            if (!resp.IsSuccessStatusCode)
                return BadRequest("Erreur stock API");

            var article = await resp.Content.ReadFromJsonAsync<ArticleDTO>();
            if (item.Quantity + 1 > article.QuantiteStock)
                return BadRequest($"❌ Stock insuffisant (max = {article.QuantiteStock})");

            item.Quantity++;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ======================================================
        // ➖ DIMINUER QUANTITÉ
        // ======================================================
        [HttpPut("dec/{articleId}")]
        public async Task<IActionResult> DecreaseItem(int articleId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int clientId = int.Parse(userIdString);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null)
                return NotFound("Panier introuvable");

            var item = cart.Items.FirstOrDefault(i => i.ArticleId == articleId);
            if (item == null)
                return NotFound("Article introuvable dans panier");

            if (item.Quantity > 1)
                item.Quantity--;
            else
                _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
