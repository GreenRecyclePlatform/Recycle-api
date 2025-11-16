using recycle.Application.DTOs.Payment;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Payment>> GetAllAsync(string? status);
        Task AddAsync(Payment payment);
        void Update(Payment payment);
    }
}
