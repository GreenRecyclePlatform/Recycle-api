using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
namespace recycle.Infrastructure.Repositories;

//public class PickupRequestRepository : IPickupRequestRepository
//{
//    private readonly AppDbContext _context;

//    public PickupRequestRepository(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<PickupRequest?> GetByIdAsync(Guid requestId)
//    {
//        return await _context.PickupRequests
//            .FirstOrDefaultAsync(pr => pr.RequestId == requestId);
//    }

//    public async Task<PickupRequest?> GetByIdWithDetailsAsync(Guid requestId)
//    {
//        return await _context.PickupRequests
//            .Include(pr => pr.User)
//            .Include(pr => pr.RequestMaterials!)
//                .ThenInclude(rm => rm.Material)
//            .FirstOrDefaultAsync(pr => pr.RequestId == requestId);
//    }

//    public async Task<IEnumerable<PickupRequest>> GetAllAsync()
//    {
//        return await _context.PickupRequests
//            .Include(pr => pr.User)
//            .OrderByDescending(pr => pr.CreatedAt)
//            .ToListAsync();
//    }

//    public async Task<IEnumerable<PickupRequest>> GetByUserIdAsync(string userId)
//    {
//        return await _context.PickupRequests
//            .Where(pr => pr.UserId == userId)
//            .OrderByDescending(pr => pr.CreatedAt)
//            .ToListAsync();
//    }

//    public async Task<IEnumerable<PickupRequest>> GetByStatusAsync(string status)
//    {
//        return await _context.PickupRequests
//            .Include(pr => pr.User)
//            .Where(pr => pr.Status == status)
//            .OrderByDescending(pr => pr.CreatedAt)
//            .ToListAsync();
//    }

//    public async Task<IEnumerable<PickupRequest>> GetFilteredAsync(
//        string? status = null,
//        string? userId = null,
//        DateTime? fromDate = null,
//        DateTime? toDate = null,
//        string? city = null,
//        int pageNumber = 1,
//        int pageSize = 10)
//    {
//        var query = _context.PickupRequests
//            .Include(pr => pr.User)
//            .Include(pr => pr.RequestMaterials!)
//                .ThenInclude(rm => rm.Material)
//            .AsQueryable();

//        if (!string.IsNullOrWhiteSpace(status))
//        {
//            query = query.Where(pr => pr.Status == status);
//        }

//        if (userId.HasValue)
//        {
//            query = query.Where(pr => pr.UserId == userId.Value);
//        }

//        if (fromDate.HasValue)
//        {
//            query = query.Where(pr => pr.CreatedAt >= fromDate.Value);
//        }

//        if (toDate.HasValue)
//        {
//            query = query.Where(pr => pr.CreatedAt <= toDate.Value);
//        }

//        if (!string.IsNullOrWhiteSpace(city))
//        {
//            query = query.Where(pr => pr.City.Contains(city));
//        }

//        return await query
//            .OrderByDescending(pr => pr.CreatedAt)
//            .Skip((pageNumber - 1) * pageSize)
//            .Take(pageSize)
//            .ToListAsync();
//    }

//    public async Task<int> GetTotalCountAsync(
//        string? status = null,
//        string? userId = null,
//        DateTime? fromDate = null,
//        DateTime? toDate = null,
//        string? city = null)
//    {
//        var query = _context.PickupRequests.AsQueryable();

//        if (!string.IsNullOrWhiteSpace(status))
//        {
//            query = query.Where(pr => pr.Status == status);
//        }

//        if (userId.HasValue)
//        {
//            query = query.Where(pr => pr.UserId == userId.Value);
//        }

//        if (fromDate.HasValue)
//        {
//            query = query.Where(pr => pr.CreatedAt >= fromDate.Value);
//        }

//        if (toDate.HasValue)
//        {
//            query = query.Where(pr => pr.CreatedAt <= toDate.Value);
//        }

//        if (!string.IsNullOrWhiteSpace(city))
//        {
//            query = query.Where(pr => pr.City.Contains(city));
//        }

//        return await query.CountAsync();
//    }

//    public async Task<PickupRequest> CreateAsync(PickupRequest pickupRequest)
//    {
//        // Ensure GUID is set (it should be from default value)
//        if (pickupRequest.RequestId == Guid.Empty)
//        {
//            pickupRequest.RequestId = Guid.NewGuid();
//        }

//        pickupRequest.CreatedAt = DateTime.UtcNow;
//        pickupRequest.Status = "Pending";

//        await _context.PickupRequests.AddAsync(pickupRequest);
//        await _context.SaveChangesAsync();

//        return pickupRequest;
//    }

//    public async Task<PickupRequest> UpdateAsync(PickupRequest pickupRequest)
//    {
//        _context.PickupRequests.Update(pickupRequest);
//        await _context.SaveChangesAsync();

//        return pickupRequest;
//    }

//    public async Task<bool> DeleteAsync(Guid requestId)
//    {
//        var pickupRequest = await GetByIdAsync(requestId);
//        if (pickupRequest == null)
//        {
//            return false;
//        }

//        _context.PickupRequests.Remove(pickupRequest);
//        await _context.SaveChangesAsync();

//        return true;
//    }

//    public async Task<bool> UpdateStatusAsync(Guid requestId, string newStatus)
//    {
//        var pickupRequest = await GetByIdAsync(requestId);
//        if (pickupRequest == null)
//        {
//            return false;
//        }

//        pickupRequest.Status = newStatus;

//        if (newStatus == "Completed")
//        {
//            pickupRequest.CompletedAt = DateTime.UtcNow;
//        }

//        await _context.SaveChangesAsync();

//        return true;
//    }

//    public async Task<bool> UpdateTotalAmountAsync(Guid requestId, decimal totalAmount)
//    {
//        var pickupRequest = await GetByIdAsync(requestId);
//        if (pickupRequest == null)
//        {
//            return false;
//        }

//        pickupRequest.TotalAmount = totalAmount;
//        await _context.SaveChangesAsync();

//        return true;
//    }
//}