using ClientsAPI.Data;
using ClientsAPI.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ClientsDbContext _context;

        public ClientController(ClientsDbContext context)
        {
            _context = context;
        }

        // 🔹 Accessible uniquement à l’Admin (par ex. SAV)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Clients.ToListAsync());
        }

        // 🔹 Le client authentifié peut accéder à son propre profil
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (userId == null || userId != id.ToString())
            {
                return Unauthorized("Vous ne pouvez accéder qu'à votre propre profil.");
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            return Ok(client);
        }

        // 🔹 Admin seulement (pas le client — inscription doit se faire via AuthAPI)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Client client)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return Ok(client);
        }
    }
}
