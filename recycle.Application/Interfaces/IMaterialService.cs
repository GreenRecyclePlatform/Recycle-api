//using recycle.Application.DTOs.Materials;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace recycle.Application.Interfaces
//{
//    public interface IMaterialService
//    {
//        Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync(bool includeInactive = false);
//        Task<MaterialDto?> GetMaterialByIdAsync(Guid id);
//        Task<MaterialDto> CreateMaterialAsync(MaterialDto dto);
//        Task<MaterialDto> UpdateMaterialAsync(Guid id, MaterialDto dto);
//        Task<bool> DeleteMaterialAsync(Guid id);
//        Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm, bool onlyActive = true);
//    }
//}
using recycle.Application.DTOs.Materials;
using recycle.Application.DTOs.RequestMaterials;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IMaterialService
    {
        Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync(bool includeInactive = false);
        Task<MaterialDto?> GetMaterialByIdAsync(Guid id);
        Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto dto);
        Task<MaterialDto> UpdateMaterialAsync(Guid id, UpdateMaterialDto dto);
        Task<Material> UpdateMaterialImageAsync(Guid id, UpdateMaterialImageDto imageDto);
        Task<bool> DeleteMaterialAsync(Guid id);
        Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm, bool onlyActive = true);
    }
}
