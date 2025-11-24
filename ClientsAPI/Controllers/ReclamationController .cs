using ClientsAPI.Data;
using ClientsAPI.DTOs;
using ClientsAPI.models;
using ClientsAPI.models;
using ClientsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReclamationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reclamation = new Reclamation
            {
                Objet = dto.Objet,
                Description = dto.Description,
                ClientId = dto.ClientId
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
