namespace ArticlesAPI.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime DateAchat { get; set; }
        public int DureeGarantieMois { get; set; } // ex: 24 mois
                                                   // 🟢 Ajouter cette ligne (comme ton champ Image dans MVC)
        public string? ImageUrl { get; set; }

        // 🔎 Propriété calculée
        public bool EstSousGarantie =>
            DateTime.Now <= DateAchat.AddMonths(DureeGarantieMois);
    }
}
