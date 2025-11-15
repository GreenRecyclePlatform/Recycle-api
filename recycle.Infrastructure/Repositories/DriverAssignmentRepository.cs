using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class DriverAssignmentRepository : IDriverAssignmentRepository
    {
        private readonly AppDbContext _context;

        public DriverAssignmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DriverAssignment?> GetByIdAsync(Guid assignmentId)
        {
            return await _context.DriverAssignments
                .Include(da => da.PickupRequest).ThenInclude(pr => pr.Address)
                .Include(da => da.Driver).ThenInclude(d => d.User)
                .Include(da => da.AssignedByAdmin)
                .FirstOrDefaultAsync(da => da.AssignmentId == assignmentId);
        }

        public async Task<DriverAssignment?> GetActiveByRequestIdAsync(Guid requestId)
        {
            return await _context.DriverAssignments
                .Include(da => da.PickupRequest)
                .FirstOrDefaultAsync(da =>
                    da.RequestId == requestId &&
                    da.IsActive);
        }

        public async Task<List<DriverAssignment>> GetByDriverIdAsync(
     Guid driverId, AssignmentStatus? status = null)
        {
            var query = _context.DriverAssignments
                .Include(da => da.PickupRequest)
                .Include(da => da.Driver)
                .Include(da => da.AssignedByAdmin)
                .Where(da => da.DriverId == driverId);

            //if filter use
            if (status.HasValue)
                query = query.Where(da => da.Status == status.Value);

            // order
            return await query
                .OrderByDescending(da => da.AssignedAt)
                .ToListAsync();
        }


        public async Task<List<DriverAssignment>> GetHistoryByRequestIdAsync(Guid requestId)
        {
            return await _context.DriverAssignments
                .Include(da => da.Driver)
                .Include(da => da.AssignedByAdmin)
                .Include(da => da.PickupRequest)
                .Where(da => da.RequestId == requestId)
                .OrderByDescending(da => da.AssignedAt)
                .ToListAsync();
        }

        public async Task AddAsync(DriverAssignment assignment)
        {
            await _context.DriverAssignments.AddAsync(assignment);
        }

        public async Task UpdateAsync(DriverAssignment assignment)
        {
            _context.DriverAssignments.Update(assignment);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

       
    }
}
