using AuthAPI.data;
using AuthAPI.DTOs;
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IPasswordHasher<AuthUser> _passwordHasher;
        private readonly IConfiguration _config;

        public AuthController(AuthDbContext context, IPasswordHasher<AuthUser> passwordHasher, IConfiguration config)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        // 🔹 Client registration (public)
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Cet email est déjà utilisé.");

            var user = new AuthUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = "Client"  // 👈 par défaut
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Client créé avec succès.");
        }

        // 🔥 Admin registration (nécessite token Admin)
        [HttpPost("registerAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Cet email est déjà utilisé.");

            var user = new AuthUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = "Admin" // 👈 ADMIN
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Administrateur créé avec succès.");
        }

        // 🔹 Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Email ou mot de passe invalide.");

            if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
                return Unauthorized("Email ou mot de passe invalide.");

            var token = GenerateJwtToken(user, out DateTime expires);

            return Ok(new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Expiration = expires
            });
        }

        // 🔹 Profil utilisateur authentifié
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Utilisateur introuvable.");

            return Ok(new { user.Id, user.FullName, user.Email, user.Role });
        }

        // ⚙ Génération du token JWT
        private string GenerateJwtToken(AuthUser user, out DateTime expires)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
