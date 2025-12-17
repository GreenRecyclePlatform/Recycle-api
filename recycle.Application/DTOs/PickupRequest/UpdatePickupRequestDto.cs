namespace recycle.Application.DTOs.PickupRequest;

public class UpdatePickupRequestDto
{
    public Guid AddressId { get; set; }
    public DateTime PreferredPickupDate { get; set; }
    public string? Notes { get; set; }
}
