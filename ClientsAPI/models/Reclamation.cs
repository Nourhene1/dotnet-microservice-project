using ClientsAPI.Models;

namespace ClientsAPI.models
{
    public class Reclamation
    {
        public int Id { get; set; }
        public string Objet { get; set; }
        public string Description { get; set; }
        public DateTime DateReclamation { get; set; } = DateTime.Now;

        // 👉 Relation
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public EtatReclamation Etat { get; set; } = EtatReclamation.Ouverte;
    }
}
