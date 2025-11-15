using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using recycle.Domain.Entities;

namespace recycle.Application.Interfaces.IRepository
{
    /// <summary>
    /// Repository interface for Review entity operations
    /// </summary>
    public interface IReviewRepository
    {
        // ==================== BASIC CRUD ====================

        /// <summary>
        /// Add a new review
        /// </summary>
        Task AddAsync(Review review);

        /// <summary>
        /// Update an existing review
        /// </summary>
        Task UpdateAsync(Review review);

        /// <summary>
        /// Remove a review
        /// </summary>
        Task RemoveAsync(Review review);

        // ==================== QUERIES ====================

        /// <summary>
        /// Get a single review by filter with optional includes
        /// </summary>
        Task<Review> GetAsync(
            Expression<Func<Review, bool>> filter,
            Func<IQueryable<Review>, IQueryable<Review>>? includes = null);

        /// <summary>
        /// Get all reviews with optional filtering, ordering, and pagination
        /// </summary>
        Task<IEnumerable<Review>> GetAll(
            Expression<Func<Review, bool>>? filter = null,
            Func<IQueryable<Review>, IQueryable<Review>>? includes = null,
            Func<IQueryable<Review>, IOrderedQueryable<Review>>? orderBy = null,
            int pageSize = 0,
            int pageNumber = 0);

        // ==================== GET BY ID ====================

        /// <summary>
        /// Get review by Guid ID
        /// </summary>
        Task<Review> GetByIdAsync(Guid id);

        // ==================== UTILITY METHODS ====================

        /// <summary>
        /// Count reviews matching a filter
        /// </summary>
        Task<int> CountAsync(Expression<Func<Review, bool>>? filter = null);

        /// <summary>
        /// Check if any review matches a filter
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<Review, bool>> filter);

        // ==================== SPECIALIZED QUERIES ====================

        /// <summary>
        /// Get all reviews for a specific driver
        /// </summary>
        Task<IEnumerable<Review>> GetReviewsByDriverIdAsync(Guid driverId, int page, int pageSize);

        /// <summary>
        /// Get all reviews by a specific user (reviewer)
        /// </summary>
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId, int page, int pageSize);

        /// <summary>
        /// Get review by request ID
        /// </summary>
        Task<Review> GetReviewByRequestIdAsync(Guid requestId);

        /// <summary>
        /// Get all flagged reviews
        /// </summary>
        Task<IEnumerable<Review>> GetFlaggedReviewsAsync();
    }
}