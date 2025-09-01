using System;

namespace StarWars.Model
{
    public class Order
    {
        public Guid Id { get; set; }
        public string PharmacyId { get; set; } = string.Empty;
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalCents { get; set; }
        public int ItemCount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool NeedsReview { get; set; }
    }
}
