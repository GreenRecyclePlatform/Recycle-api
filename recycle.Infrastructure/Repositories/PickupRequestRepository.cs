using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Repositories;

public class PickupRequestRepository : IPickupRequestRepository
{
    private readonly AppDbContext _context;

    public PickupRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PickupRequest?> GetByIdAsync(Guid requestId)
    {
        return await _context.PickupRequests
            .FirstOrDefaultAsync(pr => pr.RequestId == requestId);
    }

    public async Task<PickupRequest?> GetByIdWithDetailsAsync(Guid requestId)
    {
        return await _context.PickupRequests
            .Include(pr => pr.User)
            .Include(pr => pr.Address)
            .Include(pr => pr.RequestMaterials!)
                .ThenInclude(rm => rm.Material)
            .FirstOrDefaultAsync(pr => pr.RequestId == requestId);
    }

    public async Task<IEnumerable<PickupRequest>> GetAllAsync()
    {
        return await _context.PickupRequests
            .Include(pr => pr.User)
            .Include(pr => pr.Address)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PickupRequest>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PickupRequests
            .Where(pr => pr.UserId == userId)
            .Include(pr => pr.Address)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PickupRequest>> GetByStatusAsync(string status)
    {
        return await _context.PickupRequests
            .Include(pr => pr.User)
            .Include(pr => pr.Address)
            .Where(pr => pr.Status == status)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PickupRequest>> GetFilteredAsync(
        string? status = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? governorate = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.PickupRequests
            .Include(pr => pr.User)
            .Include(pr => pr.Address)
            .Include(pr => pr.RequestMaterials!)
                .ThenInclude(rm => rm.Material)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(pr => pr.Status == status);
        }

        if (userId.HasValue)
        {
            query = query.Where(pr => pr.UserId == userId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(pr => pr.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(pr => pr.CreatedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(governorate))
        {
            query = query.Where(pr => pr.Address.Governorate.Contains(governorate));
        }

        return await query
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? status = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? governorate = null)
    {
        var query = _context.PickupRequests
            .Include(pr => pr.Address)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(pr => pr.Status == status);
        }

        if (userId.HasValue)
        {
            query = query.Where(pr => pr.UserId == userId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(pr => pr.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(pr => pr.CreatedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(governorate))
        {
            query = query.Where(pr => pr.Address.Governorate.Contains(governorate));
        }

        return await query.CountAsync();
    }

    public async Task<PickupRequest> CreateAsync(PickupRequest pickupRequest)
    {
        // Ensure GUID is set
        if (pickupRequest.RequestId == Guid.Empty)
        {
            pickupRequest.RequestId = Guid.NewGuid();
        }

        pickupRequest.CreatedAt = DateTime.UtcNow;
        pickupRequest.Status = "Waiting";

        await _context.PickupRequests.AddAsync(pickupRequest);
        await _context.SaveChangesAsync();

        return pickupRequest;
    }

    public async Task<PickupRequest> UpdateAsync(PickupRequest pickupRequest)
    {
        _context.PickupRequests.Update(pickupRequest);
        await _context.SaveChangesAsync();

        return pickupRequest;
    }

    public async Task<bool> DeleteAsync(Guid requestId)
    {
        var pickupRequest = await GetByIdAsync(requestId);
        if (pickupRequest == null)
        {
            return false;
        }

        _context.PickupRequests.Remove(pickupRequest);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid requestId, string newStatus)
    {
        var pickupRequest = await GetByIdAsync(requestId);
        if (pickupRequest == null)
        {
            return false;
        }

        pickupRequest.Status = newStatus;

        if (newStatus == "Completed")
        {
            pickupRequest.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateTotalAmountAsync(Guid requestId, decimal totalAmount)
    {
        var pickupRequest = await GetByIdAsync(requestId);
        if (pickupRequest == null)
        {
            return false;
        }

        pickupRequest.TotalAmount = totalAmount;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<PickupRequest>> GetWaitingRequests(string status)
    {
        return await _context.PickupRequests
            .Where(pr => pr.Status == status)
            .Include(pr => pr.User)
            .Include(pr => pr.Address)
            .Include(pr => pr.RequestMaterials!)
                .ThenInclude(rm => rm.Material)
            .ToListAsync();

    }
}