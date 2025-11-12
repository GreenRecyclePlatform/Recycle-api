namespace recycle.Domain.Entities
{
    public class DriverProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string profileImageUrl { get; set; }
        public string idNumber { get; set; }
        public decimal Rating { get; set; } = 0;
        public int ratingCount { get; set; } = 0;
        public bool IsAvailable { get; set; }
        public int TotalTrips { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}