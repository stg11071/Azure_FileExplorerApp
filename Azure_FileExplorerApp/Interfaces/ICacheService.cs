namespace Azure_FileExplorerApp;

public interface ICacheService
{
    Task AddCacheData<T>(string key,  T value);

    Task<T> GetCacheData<T>(string key);

    Task RemoveCacheData(string key);
}

