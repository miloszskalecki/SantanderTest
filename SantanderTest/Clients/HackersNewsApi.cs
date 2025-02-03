namespace SantanderTest.Clients;

// could use refit framework instead
sealed class HackerNewsApi(HttpClient httpClient) : IHackerNewsApi
{
    public Task<List<long>?> GetBestStoriesAsync()
        => httpClient.GetFromJsonAsync<List<long>>("beststories.json");

    public Task<HackerNewsStory?> GetStoryAsync(long id)
        => httpClient.GetFromJsonAsync<HackerNewsStory>($"item/{id}.json");
}
