using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Review entity operations
    /// </summary>
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Review> _dbSet;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Review>();
        }

        // ==================== BASIC CRUD ====================

        public async Task AddAsync(Review review)
        {
            await _dbSet.AddAsync(review);
        }

        public async Task UpdateAsync(Review review)
        {
            _dbSet.Update(review);
            await Task.CompletedTask;
        }

        public async Task RemoveAsync(Review review)
        {
            _dbSet.Remove(review);
            await Task.CompletedTask;
        }

        // ==================== QUERIES ====================

        public async Task<Review> GetAsync(
            Expression<Func<Review, bool>> filter,
            Func<IQueryable<Review>, IQueryable<Review>>? includes = null)
        {
            IQueryable<Review> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null)
            {
                query = includes(query);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Review>> GetAll(
            Expression<Func<Review, bool>>? filter = null,
            Func<IQueryable<Review>, IQueryable<Review>>? includes = null,
            Func<IQueryable<Review>, IOrderedQueryable<Review>>? orderBy = null,
            int pageSize = 0,
            int pageNumber = 0)
        {
            IQueryable<Review> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null)
            {
                query = includes(query);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (pageNumber > 0)
            {
                if (pageSize > 50)
                {
                    pageSize = 50;
                }
                query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize);
            }

            return await query.ToListAsync();
        }

        // ==================== GET BY ID ====================

        public async Task<Review> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        // ==================== UTILITY METHODS ====================

        public async Task<int> CountAsync(Expression<Func<Review, bool>>? filter = null)
        {
            if (filter == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(filter);
        }

        public async Task<bool> AnyAsync(Expression<Func<Review, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }

        // ==================== SPECIALIZED QUERIES ====================

        public async Task<IEnumerable<Review>> GetReviewsByDriverIdAsync(Guid driverId, int page, int pageSize)
        {
            return await _dbSet
                .Where(r => r.RevieweeId == driverId && !r.IsHidden)
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                    .ThenInclude(u => u.DriverProfile)
                .Include(r => r.PickupRequest)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId, int page, int pageSize)
        {
            return await _dbSet
                .Where(r => r.ReviewerId == userId)
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                    .ThenInclude(u => u.DriverProfile)
                .Include(r => r.PickupRequest)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Review> GetReviewByRequestIdAsync(Guid requestId)
        {
            return await _dbSet
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                    .ThenInclude(u => u.DriverProfile)
                .Include(r => r.PickupRequest)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }

        public async Task<IEnumerable<Review>> GetFlaggedReviewsAsync()
        {
            return await _dbSet
                .Where(r => r.IsFlagged)
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                    .ThenInclude(u => u.DriverProfile)
                .Include(r => r.PickupRequest)
                .OrderByDescending(r => r.FlaggedAt)
                .ToListAsync();
        }
    }
}