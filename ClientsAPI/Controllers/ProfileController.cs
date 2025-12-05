using ClientsAPI.Data;
using ClientsAPI.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly ClientsDbContext _context;

    public ProfileController(ClientsDbContext context)
    {
        _context = context;
    }

    // 🔹 Récupérer le profil de l’utilisateur connecté
  
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (email == null)
            return Unauthorized("Pas d’email dans le token");

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);

        if (client == null)
            return NotFound("Client introuvable");

        return Ok(client);
    }



    // 🔹 Modifier uniquement téléphone + adresse
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        if (email == null)
            return Unauthorized();

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);

        if (client == null)
            return NotFound("Client introuvable");

        client.Telephone = dto.Telephone ?? client.Telephone;
        client.Adresse = dto.Adresse ?? client.Adresse;

        await _context.SaveChangesAsync();

        return Ok("Profil mis à jour !");
    }

}
