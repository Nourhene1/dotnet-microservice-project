using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ArticlesAPI.DTOs
{
    public class ArticleCreateDto
    {
        [Required]
        public string Nom { get; set; }

        [Required]
        public string Reference { get; set; }

        public string Description { get; set; }

        public DateTime DateAchat { get; set; }

        public int DureeGarantieMois { get; set; }

        public IFormFile? ImageFile { get; set; } // 🔥 fichier envoyé via Swagger
    }
}
