namespace recycle.Application.DTOs.PickupRequest;

public class PickupRequestResponseDto
{
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid AddressId { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public DateTime PreferredPickupDate { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }
    public decimal TotalEstimatedWeight { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<MaterialItemDto> Materials { get; set; } = new();
}

public class MaterialItemDto
{
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal EstimatedWeight { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal TotalAmount { get; set; }
}