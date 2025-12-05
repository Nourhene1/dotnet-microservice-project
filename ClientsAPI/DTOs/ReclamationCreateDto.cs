using System.ComponentModel.DataAnnotations;

namespace ClientsAPI.DTOs
{
    public class ReclamationCreateDto
    {
        [Required]
        public string Objet { get; set; }

        [Required]
        public string Description { get; set; }

    }
}
