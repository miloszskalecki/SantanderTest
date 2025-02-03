namespace SantanderTest.Services;

interface IStoryService
{
    Task<IEnumerable<Story>> GetBestStoriesAsync(int count);
}
