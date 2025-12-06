namespace ArticlesAPI.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public DateTime DateAchat { get; set; }
        public int DureeGarantieMois { get; set; }
        public string? ImageUrl { get; set; }

        // 🆕 STOCK
        public int QuantiteStock { get; set; }

        // 🆕 PRIX
        public decimal PrixUnitaire { get; set; }

        // Garantie
        public bool EstSousGarantie =>
            DateTime.Now <= DateAchat.AddMonths(DureeGarantieMois);
    }
}
