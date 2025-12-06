using Microsoft.AspNetCore.Http;
using System;
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

        // ⭐⭐⭐ AJOUT QUANTITÉ EN STOCK
        [Required]
        public int QuantiteStock { get; set; }

        // ⭐⭐⭐ AJOUT PRIX UNITAIRE
        [Required]
        public decimal PrixUnitaire { get; set; }

        // 🔥 fichier envoyé via Swagger
        public IFormFile? ImageFile { get; set; }
    }
}
