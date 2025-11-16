using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces.IRepository
{
    public interface IDriverAssignmentRepository
    {


      

        Task<DriverAssignment?> GetActiveByRequestIdAsync(Guid requestId);
        Task<List<DriverAssignment>> GetHistoryByRequestIdAsync(Guid requestId);
        // Basic CRUD
        Task<DriverAssignment?> GetByIdAsync(Guid assignmentId);
        Task<List<DriverAssignment>> GetByDriverIdAsync(Guid driverId, AssignmentStatus? status = null);
        Task AddAsync(DriverAssignment assignment);
        Task UpdateAsync(DriverAssignment assignment);

        // Save changes
        Task<bool> SaveChangesAsync();
    }


   
}

