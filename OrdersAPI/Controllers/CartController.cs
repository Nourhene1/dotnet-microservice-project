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
    [Authorize(Roles = "Client")]
    public class CartController : ControllerBase
    {
        private readonly OrdersDbContext _context;

        public CartController(OrdersDbContext context)
        {
            _context = context;
        }

        // ✅ GET api/Cart
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string email = User.FindFirstValue(ClaimTypes.Email);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null)
            {
                cart = new Cart
                {
                    ClientId = clientId,
                    ClientEmail = email,
                    Items = new List<CartItem>()
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return Ok(cart);
        }

        // ✅ POST api/Cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string email = User.FindFirstValue(ClaimTypes.Email);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null)
            {
                cart = new Cart
                {
                    ClientId = clientId,
                    ClientEmail = email,
                    Items = new List<CartItem>()
                };

                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items
                .FirstOrDefault(i => i.ArticleId == dto.ArticleId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ArticleId = dto.ArticleId,
                    ArticleName = dto.ArticleName,
                    UnitPrice = dto.UnitPrice,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok(cart);
        }

        // ✅ PUT api/Cart/items/{itemId}
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(
            int itemId,
            [FromBody] UpdateCartItemDto dto)
        {
            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i =>
                    i.Id == itemId &&
                    i.Cart != null &&
                    i.Cart.ClientId == clientId
                );

            if (item == null)
                return NotFound("Item introuvable.");

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            return Ok(item);
        }

        // ✅ DELETE api/Cart/items/{itemId}
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i =>
                    i.Id == itemId &&
                    i.Cart != null &&
                    i.Cart.ClientId == clientId
                );

            if (item == null)
                return NotFound("Item introuvable.");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE api/Cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            int clientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

            if (cart == null || !cart.Items.Any())
                return NoContent();

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
