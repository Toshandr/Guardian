namespace Guardian.Api.Services;

// Минимальный интерфейс для кеша, чтобы его можно было подменить в тестах.
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, int seconds);
    Task RemoveAsync(string key);
}
