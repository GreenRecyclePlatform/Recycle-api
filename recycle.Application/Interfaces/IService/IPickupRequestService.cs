using recycle.Application.DTOs.PickupRequest;

namespace recycle.Application.Interfaces.IService;

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
    Task<bool> CanChangeStatus(string currentStatus, string newStatus);
}