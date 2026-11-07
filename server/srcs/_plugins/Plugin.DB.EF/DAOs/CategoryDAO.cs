using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.Api.Category;
using Plugin.Database.DB;
using Plugin.Database.Mail;
using WingsAPI.Data.Api.Category;
using WingsEmu.DTOs.Mails;

namespace Plugin.Database.Category
{
    public class CategoryDAO : ICategoryDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbCategoryEntity, CategoryDTO> _mapper;
        private readonly IGenericAsyncLongRepository<CategoryDTO> _repository;

        public CategoryDAO(IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<CategoryDTO> repository, IMapper<DbCategoryEntity, CategoryDTO> mapper)
        {
            _contextFactory = contextFactory;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<CategoryDTO> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<CategoryDTO>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<CategoryDTO> SaveAsync(CategoryDTO obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<CategoryDTO>> SaveAsync(IReadOnlyList<CategoryDTO> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id)
        {
            await _repository.DeleteByIdAsync(id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await _repository.DeleteByIdsAsync(ids);
        }
    }
}
