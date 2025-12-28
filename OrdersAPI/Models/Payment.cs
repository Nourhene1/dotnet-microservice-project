namespace OrdersAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string Method { get; set; } = "Cash"; // "Card", "Virement"…
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Succeeded = 2,
        Failed = 3
    }
}
