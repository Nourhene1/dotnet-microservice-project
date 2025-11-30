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
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine("🔎 ID JWT = " + userId);

        if (userId == null) return Unauthorized("Aucun ID dans le token.");

        var client = await _context.Clients.FindAsync(int.Parse(userId));
        Console.WriteLine("🟢 Client trouvé = " + client);

        return client != null ? Ok(client) : NotFound("Utilisateur introuvable.");
    }


    // 🔹 Modifier uniquement téléphone + adresse
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var client = await _context.Clients.FindAsync(int.Parse(userId));
        if (client == null)
            return NotFound();

        client.Telephone = dto.Telephone ?? client.Telephone;
        client.Adresse = dto.Adresse ?? client.Adresse;

        await _context.SaveChangesAsync();
        return Ok("Profil mis à jour !");
    }
}
