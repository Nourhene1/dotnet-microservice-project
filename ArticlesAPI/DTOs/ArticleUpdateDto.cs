using Microsoft.AspNetCore.Http;
using System;

namespace ArticlesAPI.DTOs
{
    public class ArticleUpdateDto
    {
        public string? Nom { get; set; }
        public string? Reference { get; set; }
        public string? Description { get; set; }
        public DateTime? DateAchat { get; set; }
        public int? DureeGarantieMois { get; set; }

        // 🖼️ Image facultative
        public IFormFile? ImageFile { get; set; }
    }
}
