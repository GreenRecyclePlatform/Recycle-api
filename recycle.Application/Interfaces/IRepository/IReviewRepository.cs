using recycle.Domain.Entities;

namespace recycle.Application.Interfaces.IRepository;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByRequestIdAsync(Guid requestId);
    Task<IEnumerable<Review>> GetAllAsync();
    Task<IEnumerable<Review>> GetByDriverIdAsync(Guid driverId);
    Task<IEnumerable<Review>> GetByCustomerIdAsync(Guid customerId);
}