namespace recycle.Application.DTOs.Reviews;

public class ReviewDto
{
    public Guid ReviewId { get; set; }
    public Guid RequestId { get; set; }
    public Guid DriverId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsHidden { get; set; }
    public string? FlagReason { get; set; }
    public DateTime? FlaggedAt { get; set; }
    public string CustomerName { get; set; }
    public string DriverName { get; set; }
}