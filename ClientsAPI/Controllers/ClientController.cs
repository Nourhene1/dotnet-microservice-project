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
        private readonly HttpClient _http;

        public ClientController(ClientsDbContext context, HttpClient http)
        {
            _context = context;
            _http = http;
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

            // 1️⃣ Enregistrement du client métier
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // 2️⃣ Construction des données pour AuthAPI
            var password = client.Telephone; // ou random si tu veux
            var authUserPayload = new
            {
                FullName = $"{client.Nom} {client.Prenom}",
                Email = client.Email,
                Password = password,
                Role = "Client"
            };

            // 3️⃣ Appel HTTP vers AuthAPI
            var response = await _http.PostAsJsonAsync(
                "https://localhost:7273/api/auth/register",  // URL AuthAPI
                authUserPayload
            );

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Le client est créé, mais la création Auth a échoué.");
            }

            return Ok(new
            {
                client,
                message = "Client créé + compte Auth créé automatiquement",
                loginPassword = password  // 💡 utile à afficher à l’admin
            });
        }


        

    }

}
