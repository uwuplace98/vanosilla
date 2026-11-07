using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using WingsAPI.Data.Api.Shop;

namespace Plugin.Database.Api.Shop
{
    public class WebShopItemDAO : IWebShopItemDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<DbShopItemEntity, WebShopItemDTO> _mapper;
        private readonly IGenericAsyncLongRepository<WebShopItemDTO> _repository;

        public WebShopItemDAO(IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<WebShopItemDTO> repository, IMapper<DbShopItemEntity, WebShopItemDTO> mapper)
        {
            _contextFactory = contextFactory;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WebShopItemDTO>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<WebShopItemDTO> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        // Implement additional methods for saving, updating, and deleting shop items
    }
}
