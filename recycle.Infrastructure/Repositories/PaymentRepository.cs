// recycle.Infrastructure/Repositories/PaymentRepository.cs

using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync(string? status)
        {
            var query = _context.Payments
                .Include(p => p.RecipientUser) // Include user for UserName
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.PaymentStatus == status);

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.RecipientUser)
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public void Update(Payment payment)
        {
            _context.Payments.Update(payment);
        }

        //  NEW: Get payments by user ID
        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Payments
                .Include(p => p.RecipientUser)
                .Where(p => p.RecipientUserID == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        //  NEW: Get queryable for complex filtering
        public IQueryable<Payment> GetQueryable()
        {
            return _context.Payments
                .Include(p => p.RecipientUser)
                .AsQueryable();
        }
    }
}