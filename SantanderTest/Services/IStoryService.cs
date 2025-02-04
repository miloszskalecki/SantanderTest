namespace SantanderTest.Services;

internal interface IStoryService
{
    Task<IEnumerable<Story>> GetBestStoriesAsync(int count);
}
