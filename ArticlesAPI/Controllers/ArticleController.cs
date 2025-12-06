using ArticlesAPI.Data;
using ArticlesAPI.DTOs;
using ArticlesAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticlesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ArticlesDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArticleController(ArticlesDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 🔓 Public - accessible sans token
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Articles.ToListAsync());
        }

        // 🔓 Public
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound("Article introuvable !");
            return Ok(article);
        }

        // 🔐 CREATE - ADMIN
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ArticleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string imagePath = null;

            // 📷 upload file
            if (dto.ImageFile != null)
            {
                string uploadsDir = Path.Combine(_env.WebRootPath, "images/articles");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                string uniqueName = Guid.NewGuid() + Path.GetExtension(dto.ImageFile.FileName);
                string filePath = Path.Combine(uploadsDir, uniqueName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                imagePath = "/images/articles/" + uniqueName;
            }

            // ⭐ création article FULL DATA
            var article = new Article
            {
                Nom = dto.Nom,
                Reference = dto.Reference,
                Description = dto.Description,
                DateAchat = dto.DateAchat,
                DureeGarantieMois = dto.DureeGarantieMois,
                QuantiteStock = dto.QuantiteStock,   // ⭐ AJOUTÉ
                PrixUnitaire = dto.PrixUnitaire,     // ⭐ AJOUTÉ
                ImageUrl = imagePath
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
        }

        // 🔓 Vérifier garantie
        [AllowAnonymous]
        [HttpGet("{id}/sousGarantie")]
        public async Task<IActionResult> EstSousGarantie(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound("Article introuvable !");
            return Ok(article.EstSousGarantie);
        }

        // ✏ UPDATE - ADMIN
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ArticleUpdateDto dto)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound("Article introuvable");

            // 🔁 MAPPING FIELDS OPTIONNELS
            if (dto.Nom != null) article.Nom = dto.Nom;
            if (dto.Reference != null) article.Reference = dto.Reference;
            if (dto.Description != null) article.Description = dto.Description;
            if (dto.DateAchat.HasValue) article.DateAchat = dto.DateAchat.Value;
            if (dto.DureeGarantieMois.HasValue) article.DureeGarantieMois = dto.DureeGarantieMois.Value;

            // ⭐⭐ UPDATE STOCK + PRIX
            if (dto.QuantiteStock.HasValue) article.QuantiteStock = dto.QuantiteStock.Value;
            if (dto.PrixUnitaire.HasValue) article.PrixUnitaire = dto.PrixUnitaire.Value;

            // 📷 upload image
            if (dto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(article.ImageUrl))
                {
                    string oldImagePath = Path.Combine(_env.WebRootPath, article.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                string uploadsDir = Path.Combine(_env.WebRootPath, "images/articles");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                string uniqueName = Guid.NewGuid() + Path.GetExtension(dto.ImageFile.FileName);
                string filePath = Path.Combine(uploadsDir, uniqueName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                article.ImageUrl = "/images/articles/" + uniqueName;
            }

            await _context.SaveChangesAsync();
            return Ok(article);
        }

        // ❌ DELETE - ADMIN
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound("Article introuvable");

            // 📷 delete image if exists
            if (!string.IsNullOrEmpty(article.ImageUrl))
            {
                string imagePath = Path.Combine(_env.WebRootPath, article.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin,Client,Technicien")]
        [HttpPut("{id}/decrease-stock")]
        public async Task<IActionResult> DecreaseStock(int id, [FromBody] int quantity)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound("Article introuvable");

            if (article.QuantiteStock < quantity)
                return BadRequest("Stock insuffisant");

            article.QuantiteStock -= quantity;
            await _context.SaveChangesAsync();

            return Ok(article.QuantiteStock);
        }

    }
}
