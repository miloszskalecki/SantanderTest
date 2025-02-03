using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SantanderTest.Clients;
using SantanderTest.Settings;

namespace SantanderTest.Services;

sealed class StoryService(
    IMemoryCache memoryCache,
    IHackerNewsClient hackerNewsClient,
    ILogger<StoryService> logger,
    IOptions<StoryServiceSettings> settings) : IStoryService
{
    public static readonly object BestStoriesKey = new();

    private readonly object _lock = new();

    public async Task<IEnumerable<Story>> GetBestStoriesAsync(int count)
    {
        if (count <= 0)
            return [];

        var bestStoryList = await GetOrAddAsync(BestStoriesKey, FetchBestStoriesAsync, settings.Value.StoryListExpiration);
        if (bestStoryList is null || bestStoryList.Count == 0)
            return [];

        var bestStoryTasks = bestStoryList
            .Take(count)
            .Select(id => GetOrAddAsync(id, () => FetchStoryAsync(id), settings.Value.StoryExpiration));

        // todo: chunkify to prevent thread pool starvation on incoming requests
        var bestStories = await Task.WhenAll(bestStoryTasks);

        return bestStories
            .Where(p => p is not null)
            .ToList()!;
    }

    private async Task<List<long>> FetchBestStoriesAsync()
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Retrieving list of best stories");

        return await hackerNewsClient.GetBestStoriesAsync() ?? [];
    }

    private async Task<Story?> FetchStoryAsync(long storyId)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Retrieving story {storyId}", storyId);

        var hackerNewsStory = await hackerNewsClient.GetStoryAsync(storyId);

        return hackerNewsStory?.ToStory();
    }

    private async Task<T?> GetOrAddAsync<T>(object key, Func<Task<T>> factory, TimeSpan expiration)
    {
        Task<T>? task = null;

        // MemoryCache.GetOrCreateAsync extension method 
        // does not work in a way most people think it does
        // it is not atomic and results in multiple executions
        // of the factory method for simultaneous callers 

        // a simple fix is to lock for a short period of time to get/insert task 
        // but not wait for its execution, guaranteeing all callers will get & await on the same task
        // better AsyncLazy<T> implementation could be used instead if given more time
        lock (_lock)
        {
            if (!memoryCache.TryGetValue(key, out task) || task is null)
                task = memoryCache.Set(key, Task.Run(factory), expiration);
        }

        // simple solution to not to cache exception tasks 
        // and retry on next service call
        try
        {
            return await task;
        }
        catch
        {
            memoryCache.Remove(key);
            throw;
        }
    }
}