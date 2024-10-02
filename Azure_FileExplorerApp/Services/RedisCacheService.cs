using StackExchange.Redis;
using System.Text.Json;
using Azure_FileExplorerApp;
using Microsoft.EntityFrameworkCore.Storage;
using IDatabase = StackExchange.Redis.IDatabase;

namespace AzureTeacherStudentSystem;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;


    public RedisCacheService(IDatabase database)
    {
        _database = database;
    }


    public async Task AddCacheData<T>(string key, T value)
    {
        await _database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry: TimeSpan.FromMinutes(60));
    }


    public async Task<T> GetCacheData<T>(string key)
    {
        if (! await _database.KeyExistsAsync(key))
        {
            return default;
        }

        var data = await _database.StringGetAsync(key);
        return JsonSerializer.Deserialize<T>(data);

    }


    public async Task RemoveCacheData(string key)
    {
        if (await _database.KeyExistsAsync(key))
        {
            await _database.KeyDeleteAsync(key);
        }

    }
}

