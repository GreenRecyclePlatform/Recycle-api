using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IRequestMaterialRepository
    {
        Task<IEnumerable<RequestMaterial>> GetByRequestIdAsync(Guid requestId);
        Task<RequestMaterial?> GetByIdAsync(Guid id);
        Task<RequestMaterial> CreateAsync(RequestMaterial requestMaterial);
        Task<IEnumerable<RequestMaterial>> CreateBulkAsync(IEnumerable<RequestMaterial> requestMaterials);
        Task<RequestMaterial> UpdateActualWeightAsync(Guid id, decimal actualWeight, string? notes);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid requestId, Guid materialId);
        Task<decimal> CalculateTotalAmountAsync(Guid requestId);
        Task<int> GetMaterialCountForRequestAsync(Guid requestId);
    }
}
