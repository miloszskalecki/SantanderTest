namespace SantanderTest.Clients;

interface IHackerNewsApi
{
    Task<List<long>?> GetBestStoriesAsync();

    Task<HackerNewsStory?> GetStoryAsync(long id);
}
