using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Review?> GetByRequestIdAsync(Guid requestId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.RequestId == requestId);
    }

    public async Task<IEnumerable<Review>> GetAllAsync()
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .Include(r => r.PickupRequest)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByDriverIdAsync(Guid driverId)
    {
        return await _context.Reviews
            .Where(r => r.RevieweeId == driverId)
            .Include(r => r.Reviewer)
            .Include(r => r.PickupRequest)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Reviews
            .Where(r => r.ReviewerId == customerId)
            .Include(r => r.Reviewee)
            .Include(r => r.PickupRequest)
            .ToListAsync();
    }
}