using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SneakerShop.WinForms.Models;

namespace SneakerShop.WinForms.Services
{
    public sealed class ApiService
    {
        private const string BaseUrl =
            "http://localhost:5000/";

        private readonly HttpClient _client;

        private readonly JsonSerializerOptions _jsonOptions =
            new()
            {
                PropertyNameCaseInsensitive = true
            };

        public static ApiService Instance { get; } = new();

        private ApiService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                using HttpResponseMessage response =
                    await _client.GetAsync(
                        "api/dashboard/summary");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AuthResponse> LoginAsync(
            LoginRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    "api/auth/login",
                    request);

            return await ReadResponseAsync<AuthResponse>(
                response);
        }

        public async Task<AuthResponse> RegisterAsync(
            RegisterRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    "api/auth/register",
                    request);

            return await ReadResponseAsync<AuthResponse>(
                response);
        }

        public async Task<List<Item>> GetItemsAsync(
            string? search = null,
            bool includeInactive = false)
        {
            string url =
                $"api/items?includeInactive={includeInactive.ToString().ToLower()}";

            if (!string.IsNullOrWhiteSpace(search))
            {
                url +=
                    $"&search={Uri.EscapeDataString(search)}";
            }

            return await GetAsync<List<Item>>(url);
        }

        public async Task<List<Item>> GetLowStockAsync()
        {
            return await GetAsync<List<Item>>(
                "api/items/low-stock");
        }

        public async Task<Item> CreateItemAsync(
            ItemRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    "api/items",
                    request);

            return await ReadResponseAsync<Item>(response);
        }

        public async Task UpdateItemAsync(
            int id,
            ItemRequest request)
        {
            using HttpResponseMessage response =
                await _client.PutAsJsonAsync(
                    $"api/items/{id}",
                    request);

            await EnsureSuccessAsync(response);
        }

        public async Task DeactivateItemAsync(int id)
        {
            using HttpResponseMessage response =
                await _client.DeleteAsync(
                    $"api/items/{id}");

            await EnsureSuccessAsync(response);
        }

        public async Task RestoreItemAsync(int id)
        {
            using StringContent content = new(
                "{}",
                Encoding.UTF8,
                "application/json");

            using HttpResponseMessage response =
                await _client.PatchAsync(
                    $"api/items/{id}/restore",
                    content);

            await EnsureSuccessAsync(response);
        }

        public async Task<DashboardSummary>
            GetDashboardAsync()
        {
            return await GetAsync<DashboardSummary>(
                "api/dashboard/summary");
        }

        public async Task<List<RestockSuggestion>>
            GetRestockSuggestionsAsync()
        {
            return await GetAsync<List<RestockSuggestion>>(
                "api/dashboard/restock-suggestions");
        }

        public async Task<List<InventoryTransactionRecord>>
            GetTransactionsAsync()
        {
            return await GetAsync<
                List<InventoryTransactionRecord>>(
                "api/inventory-transactions");
        }

        public async Task SendStockMovementAsync(
            string operation,
            StockMovementRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    $"api/inventory-transactions/{operation}",
                    request);

            await EnsureSuccessAsync(response);
        }

        public async Task<List<OrderResponse>>
            GetOrdersAsync()
        {
            return await GetAsync<List<OrderResponse>>(
                "api/orders");
        }

        public async Task<OrderResponse> GetOrderAsync(
            int id)
        {
            return await GetAsync<OrderResponse>(
                $"api/orders/{id}");
        }

        public async Task<OrderResponse> CreateOrderAsync(
            CreateOrderRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    "api/orders",
                    request);

            return await ReadResponseAsync<OrderResponse>(
                response);
        }

        public async Task CancelOrderAsync(
            int id,
            OrderActionRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    $"api/orders/{id}/cancel",
                    request);

            await EnsureSuccessAsync(response);
        }

        public async Task ReturnOrderItemAsync(
            int orderId,
            ReturnOrderItemRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    $"api/orders/{orderId}/return",
                    request);

            await EnsureSuccessAsync(response);
        }

        public async Task ExchangeOrderItemAsync(
            int orderId,
            ExchangeOrderItemRequest request)
        {
            using HttpResponseMessage response =
                await _client.PostAsJsonAsync(
                    $"api/orders/{orderId}/exchange",
                    request);

            await EnsureSuccessAsync(response);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            using HttpResponseMessage response =
                await _client.GetAsync(url);

            return await ReadResponseAsync<T>(response);
        }

        private async Task<T> ReadResponseAsync<T>(
            HttpResponseMessage response)
        {
            string content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    GetErrorMessage(content));
            }

            T? result = JsonSerializer.Deserialize<T>(
                content,
                _jsonOptions);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "The API returned an empty response.");
            }

            return result;
        }

        private async Task EnsureSuccessAsync(
            HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string content =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                GetErrorMessage(content));
        }

        private static string GetErrorMessage(
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "The server could not complete the request.";
            }

            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(content);

                JsonElement root =
                    document.RootElement;

                if (root.TryGetProperty(
                    "message",
                    out JsonElement message))
                {
                    return message.GetString() ??
                        "The request failed.";
                }

                if (root.TryGetProperty(
                    "errors",
                    out JsonElement errors))
                {
                    foreach (
                        JsonProperty property
                        in errors.EnumerateObject())
                    {
                        if (property.Value.ValueKind ==
                            JsonValueKind.Array)
                        {
                            JsonElement first =
                                property.Value
                                    .EnumerateArray()
                                    .FirstOrDefault();

                            if (first.ValueKind ==
                                JsonValueKind.String)
                            {
                                return first.GetString() ??
                                    "Validation failed.";
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // The response was not JSON.
            }

            return content;
        }
    }
}