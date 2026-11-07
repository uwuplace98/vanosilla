// ============================================
// Vanosilla Integration Tests - Auth Check-In Flow
// Testet: LoginServer ↔ BazaarServer gRPC Schnittstelle
// ============================================

using Microsoft.Extensions.Hosting;
using Microsoft.Protobuf.Reflection;
using System.Net.Http;
using System.Text;

namespace Vanosilla.Protobuf.Tests.Integration
{
    public sealed class AuthServiceIntegrationTest : IAsyncLifetime
    {
        private readonly string _bazaarEndpoint = "http://localhost:50051";
        private readonly HttpClient _httpClient;

        public AuthServiceIntegrationTest()
        {
            _httpClient = new HttpClient();
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask; // Datenbank muss vorher initianisiert sein
        }

        public async Task DisposeAsync()
        {
            await _httpClient.DisposeAsync();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task LoginRequest_AuthCheckIn_Success()
        {
            // Arrange
            var request = new Proto.Auth.LoginRequest
            {
                AccountId = "ACC_TEST_001",
                CharacterName = "TestCharacterA",
                ServerId = 1,
                LoginTime = Proto.Types.Timestamp.FromDateTime(DateTime.UtcNow)
            };

            // Act
            using var message = Proto.CSharpProto.GetRequest(new Proto.Auth.LoginRequest(), request);
            var responseBytes = await _httpClient.PostAsync($"{_bazaarEndpoint}/Auth/CheckIn", new StringContent("", Encoding.UTF8));
            var responseText = await responseBytes.Content.ReadAsStringAsync();

            // Assert
            Assert.True(responseText.Contains("Login successful"), $"AuthCheckIn-Fehler: {responseText}");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TradeRequest_Buy_Success()
        {
            // Arrange
            var request = new Proto.Auth.TradeRequest
            {
                AccountId = "ACC_TEST_001",
                ItemId = 1001,
                Quantity = 1,
                OperationType = 1,  // Buy
                Price = 5000.0f,
                SourceServerId = "SERVER_001"
            };

            // Act
            using var message = Proto.CSharpProto.GetRequest(new Proto.Auth.TradeRequest(), request);
            var responseBytes = await _httpClient.PostAsync($"{_bazaarEndpoint}/Auth/Trade", new StringContent("", Encoding.UTF8));
            var responseText = await responseBytes.Content.ReadAsStringAsync();

            // Assert
            Assert.True(responseText.Contains("Transaktion erfolgreich"), $"TradeBuy-Fehler: {responseText}");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task HealthCheck_Healthy_ReturnsTrue()
        {
            // Arrange
            var responseBytes = await _httpClient.GetAsync($"{_bazaarEndpoint}/Health");
            var responseText = await responseBytes.Content.ReadAsStringAsync();

            // Assert
            Assert.True(responseText.Contains("healthy"), $"HealthCheck-Fehler: {responseText}");
        }

        [Theory]
        [Trait("Category", "Integration")]
        [InlineData("INVALID_ACCOUNT_ID")]
        public async Task LoginRequest_InvalidAccount_Fail(string accountId)
        {
            // Arrange
            var request = new Proto.Auth.LoginRequest
            {
                AccountId = accountId,
                CharacterName = "TestCharacter",
                ServerId = 1,
                LoginTime = Proto.Types.Timestamp.FromDateTime(DateTime.UtcNow)
            };

            // Act
            using var message = Proto.CSharpProto.GetRequest(new Proto.Auth.LoginRequest(), request);
            var responseBytes = await _httpClient.PostAsync($"{_bazaarEndpoint}/Auth/CheckIn", new StringContent("", Encoding.UTF8));
            var responseText = await responseBytes.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Account nicht gefunden", responseText);
        }
    }
}
