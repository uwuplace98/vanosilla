// ============================================
// Vanosilla gRPC API - Authentication Service Implementation
// Schnittstelle: LoginServer <-> BazaarServer
// ============================================

using Microsoft.Protobuf.Reflection;
using Microsoft.Protobuf.Types;

namespace Vanosilla.Protobuf
{
    // Register Protobuf-Messages bei den C#-Classes
}

// Auth Check-In Handler
public partial class AuthService
{
    public async Task<LoginResponse> HandleAuthCheckInAsync(LoginRequest request, ServerContext context)
    {
        try
        {
            // 1. Account validisieren (Datenbank-Schema aus bazaar-schema.sql)
            var account = await _database.GetAccountByIdAsync(request.account_id);

            if (account == null || !account.IsActive)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Account '{request.account_id}' nicht gefunden oder inaktiv"
                };
            }

            // 2. Wallet-Balance abfragen
            var balanceData = await _bazaarService.GetBalanceAsync(account.id);

            // 3. Login-Timestamp aktualisieren (PostgreSQL UPDATE)
            await _database.UpdateLastLoginAsync(account.id, DateTime.UtcNow);

            return new LoginResponse
            {
                Success = true,
                Message = $"Welcome {request.character_name}!",
                WalletBalance = balanceData.Balance,
                GoldAmount = balanceData.GoldTotal,
                SilverAmount = balanceData.SilverTotal
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = $"Authentifizierungsfehler: {ex.Message}"
            };
        }
    }

    public async Task<TradeResponse> HandleTradeRequestAsync(TradeRequest request, ServerContext context)
    {
        try
        {
            // 1. Item validisieren (Datenbank-Schema aus bazaar-schema.sql)
            var item = await _inventory.GetItemByIdAsync(request.item_id);

            if (item == null || item.Rarity < GetRarityValue(request.operation_type))
            {
                return new TradeResponse
                {
                    Success = false,
                    Message = $"Item '{item.Name}' nicht gefunden oder zu selten"
                };
            }

            // 2. Preis berechnen (server-specific overrides aus .env.sample)
            var basePrice = item.BasePrice;
            var serverMin = _config.ServerMinPrice(request.server_id);
            var serverMax = _config.ServerMaxPrice(request.server_id);
            float price = basePrice.Clamp(serverMin, serverMax);

            // 3. Account-Balance prüfen (bazaar-schema.sql: bazaar.accounts table)
            var accountBalance = await _database.GetAccountAsync(request.account_id).Balance;
            if (accountBalance < price * request.Quantity)
            {
                return new TradeResponse
                {
                    Success = false,
                    Message = "Unzureichende Kredit-Balance"
                };
            }

            // 4. Transaktion durchführen (bazaar-schema.sql: bazaar.transactions table)
            var transactionId = GenerateTransactionId();
            await _database.InsertTransactionAsync(
                new {
                    TransactionId = transactionId,
                    AccountId = request.account_id,
                    Type = request.OperationType.ToString(),
                    ItemId = request.item_id,
                    Quantity = request.Quantity,
                    Price = price,
                    SourceServerId = request.SourceServerId
                });

            return new TradeResponse
            {
                Success = true,
                Message = $"Transaktion erfolgreich: {request.Quantity} x {item.Name}",
                ProcessedPrice = price,
                RemainingBalance = accountBalance - (price * request.Quantity)
            };
        }
        catch (Exception ex)
        {
            return new TradeResponse
            {
                Success = false,
                Message = $"Trading-Fehler: {ex.Message}"
            };
        }
    }

    // Helper-Methoden
    private float GetRarityValue(string operationType) => operationType switch
    {
        "buy" => 1.0f,
        "sell" => 5.0f,
        "refund" => 10.0f,
        _ => 1.0f
    };

    private string GenerateTransactionId() => $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString()[..8]}";
}
