using ClientsAPI.Data;
using ClientsAPI.DTOs;
using ClientsAPI.models;
using ClientsAPI.models;
using ClientsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClientsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReclamationController : ControllerBase
    {
        private readonly ClientsDbContext _context;

        public ReclamationController(ClientsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Reclamations.Include(r => r.Client).ToListAsync());
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReclamationCreateDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email).Value;
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);

            if (client == null)
                return BadRequest("Client introuvable");

            var reclamation = new Reclamation
            {
                Objet = dto.Objet,
                Description = dto.Description,
                ClientId = client.Id
            };

            _context.Reclamations.Add(reclamation);
            await _context.SaveChangesAsync();

            return Ok(reclamation);
        }

        [HttpPut("{id}/etat")]
        public async Task<IActionResult> ChangerEtat(int id, [FromBody] EtatReclamation nouvelEtat)
        {
            var reclamation = await _context.Reclamations.FindAsync(id);
            if (reclamation == null)
                return NotFound("Réclamation introuvable");

            reclamation.Etat = nouvelEtat;
            await _context.SaveChangesAsync();

            return Ok(reclamation);
        }

    }
}
