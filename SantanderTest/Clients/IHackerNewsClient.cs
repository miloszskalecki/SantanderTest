using Refit;

namespace SantanderTest.Clients;

internal interface IHackerNewsClient
{
    [Get("/beststories.json")]
    Task<List<long>?> GetBestStoriesAsync();

    [Get("/item/{id}.json")]
    Task<HackerNewsStory?> GetStoryAsync(long id);
}
