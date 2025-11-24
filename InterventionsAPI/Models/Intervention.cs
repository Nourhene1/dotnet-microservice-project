namespace InterventionsAPI.Models
{
    public class Intervention
    {
        public int Id { get; set; }
        public int ReclamationId { get; set; }
        public string Technicien { get; set; }
        public string Description { get; set; }
        public DateTime DateIntervention { get; set; } = DateTime.Now;
        public string Statut { get; set; } = "En attente"; // En cours, Terminée...
    }
}
