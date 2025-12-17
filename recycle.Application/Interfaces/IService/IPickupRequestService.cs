using recycle.Application.DTOs.PickupRequest;

namespace recycle.Application.Interfaces;

public interface IPickupRequestService
{
    Task<PickupRequestResponseDto?> GetByIdAsync(Guid requestId);
    Task<IEnumerable<PickupRequestResponseDto>> GetAllAsync();
    Task<IEnumerable<PickupRequestResponseDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<PickupRequestResponseDto>> GetByStatusAsync(string status);
    Task<(IEnumerable<PickupRequestResponseDto> Requests, int TotalCount)> GetFilteredAsync(PickupRequestFilterDto filter);
    Task<PickupRequestResponseDto> CreateAsync(Guid userId, CreatePickupRequestDto createDto);
    Task<PickupRequestResponseDto?> UpdateAsync(Guid requestId, UpdatePickupRequestDto updateDto);
    Task<bool> DeleteAsync(Guid requestId);
    Task<bool> UpdateStatusAsync(Guid requestId, string newStatus);
    bool CanChangeStatus(string currentStatus, string newStatus);
    Task<IEnumerable<WaitingRequestDto>> GetWaitingRequestsAsync(string status);
    
}