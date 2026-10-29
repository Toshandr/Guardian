using System.Text.Json;
using StackExchange.Redis;

namespace Guardian.Api.Services;

// Читает и записывает сериализованные данные в Redis
public class RedisCacheService(IConfiguration configuration) : ICacheService
{
    private readonly Lazy<ConnectionMultiplexer> _connection = new(() =>
        ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _connection.Value.GetDatabase().StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!);
    }

    public async Task SetAsync<T>(string key, T value, int seconds)
    {
        var json = JsonSerializer.Serialize(value);
        await _connection.Value.GetDatabase().StringSetAsync(key, json, TimeSpan.FromSeconds(seconds));
    }

    public async Task RemoveAsync(string key)
    {
        await _connection.Value.GetDatabase().KeyDeleteAsync(key);
    }
}
