using InterventionsAPI.Data;
using InterventionsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterventionsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterventionController : ControllerBase
    {
        private readonly InterventionDbContext _context;

        public InterventionController(InterventionDbContext context)
        {
            _context = context;
        }

        // 🔎 Voir toutes les interventions
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _context.Interventions.ToListAsync());

        // 🔐 Ajouter intervention (TECHNICIEN ou ADMIN)
        [Authorize(Roles = "Technicien,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Intervention intervention)
        {
            _context.Interventions.Add(intervention);
            await _context.SaveChangesAsync();
            return Ok(intervention);
        }

        // 🔧 Modifier statut
        [Authorize(Roles = "Technicien,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Intervention updated)
        {
            var intervention = await _context.Interventions.FindAsync(id);
            if (intervention == null) return NotFound();

            intervention.Description = updated.Description ?? intervention.Description;
            intervention.Statut = updated.Statut ?? intervention.Statut;

            await _context.SaveChangesAsync();
            return Ok(intervention);
        }
    }
}
