using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingsEmu.DTOs.Shops;

namespace WingsAPI.Data.Api.Shop
{
    public interface IWebShopItemDAO
    {
        Task<IEnumerable<WebShopItemDTO>> GetAllAsync();
        Task<WebShopItemDTO> GetByIdAsync(long id);
        // Additional methods for saving, updating, and deleting shop items
    }
}
