namespace recycle.Domain.Entities;

public class PickupRequest
{
    public Guid RequestId { get; set; } = Guid.NewGuid(); //here is the guid will be generated in app instead of in the database.
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
    public DateTime PreferredPickupDate { get; set; }
    public string Status { get; set; } = "Waiting"; // Waiting/Pending/Assigned/PickedUp/Completed/Cancelled
    public string? Notes { get; set; }
    public decimal TotalEstimatedWeight { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public ApplicationUser? User { get; set; }
    public ICollection<RequestMaterial>? RequestMaterials { get; set; }
    public ICollection<DriverAssignment>? DriverAssignments { get; set; }
    public ICollection<Payment>? Payments { get; set; }
    public Review? Review { get; set; }
    public Address Address {get; set;}
}
