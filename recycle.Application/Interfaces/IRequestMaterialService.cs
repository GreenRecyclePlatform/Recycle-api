using recycle.Application.DTOs.RequestMaterials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IRequestMaterialService
    {
        Task<IEnumerable<RequestMaterialDto>> GetRequestMaterialsAsync(Guid requestId);
        Task<RequestMaterialDto?> GetRequestMaterialByIdAsync(Guid id);
        Task<RequestMaterialDto> AddMaterialToRequestAsync(Guid requestId, RequestMaterialDto dto);
        Task<IEnumerable<RequestMaterialDto>> AddMaterialsToRequestAsync(Guid requestId, IEnumerable<RequestMaterialDto> dtos);
        Task<RequestMaterialDto> UpdateActualWeightAsync(Guid id, UpdateActualWeightDto dto);
        Task<bool> RemoveMaterialFromRequestAsync(Guid id);
        Task<decimal> GetRequestTotalAmountAsync(Guid requestId);
    }
}
