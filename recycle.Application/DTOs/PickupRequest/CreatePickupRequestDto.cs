namespace recycle.Application.DTOs.PickupRequest;

public class CreatePickupRequestDto
{
    public Guid AddressId { get; set; }
    public DateTime PreferredPickupDate { get; set; }
    public string? Notes { get; set; }
    public List<RequestMaterialItemDto> Materials { get; set; } = new();

    // NEW: PayPal email for payment processing
    public string PayPalEmail { get; set; } = string.Empty;

}

public class RequestMaterialItemDto
{
    public Guid MaterialId { get; set; }
    public decimal EstimatedWeight { get; set; }
}