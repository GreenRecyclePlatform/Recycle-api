namespace recycle.Application.DTOs.PickupRequest;

public class UpdatePickupRequestDto
{
    public string PickupAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public DateTime PreferredPickupDate { get; set; }
    public string? Notes { get; set; }
}