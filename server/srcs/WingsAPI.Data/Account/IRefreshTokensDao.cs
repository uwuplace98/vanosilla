using PhoenixLib.DAL;

namespace WingsAPI.Data.Account;

public interface IRefreshTokensDao : IGenericAsyncLongRepository<RefreshTokensDto>
{
}