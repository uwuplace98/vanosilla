using System.Collections.Generic;
using System.Threading.Tasks;

namespace WingsAPI.Data.Api.Category
{
    public interface ICategoryDAO
    {
        Task<IEnumerable<CategoryDTO>> GetAllAsync();
        Task<CategoryDTO> GetByIdAsync(long id);
        Task<IEnumerable<CategoryDTO>> GetByIdsAsync(IEnumerable<long> ids);
        Task<CategoryDTO> SaveAsync(CategoryDTO obj);
        Task<IEnumerable<CategoryDTO>> SaveAsync(IReadOnlyList<CategoryDTO> objs);
        Task DeleteByIdAsync(long id);
        Task DeleteByIdsAsync(IEnumerable<long> ids);
    }
}
