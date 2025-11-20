using recycle.Domain.Entities;

namespace recycle.Application.Interfaces;

public interface IPickupRequestRepository
{
    Task<PickupRequest?> GetByIdAsync(Guid requestId);
    Task<PickupRequest?> GetByIdWithDetailsAsync(Guid requestId);
    Task<IEnumerable<PickupRequest>> GetAllAsync();
    Task<IEnumerable<PickupRequest>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<PickupRequest>> GetByStatusAsync(string status);
    Task<IEnumerable<PickupRequest>> GetFilteredAsync(
        string? status = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? governorate = null,
        int pageNumber = 1,
        int pageSize = 10);
    Task<int> GetTotalCountAsync(
        string? status = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? governorate = null);
    Task<PickupRequest> CreateAsync(PickupRequest pickupRequest);
    Task<PickupRequest> UpdateAsync(PickupRequest pickupRequest);
    Task<bool> DeleteAsync(Guid requestId);
    Task<bool> UpdateStatusAsync(Guid requestId, string newStatus);
    Task<bool> UpdateTotalAmountAsync(Guid requestId, decimal totalAmount);
}