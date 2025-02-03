using SantanderTest.Clients;

namespace SantanderTest.Services;

static class StoryMapper
{
    public static Story ToStory(this HackerNewsStory hackerNewsStory) => new()
    {
        Uri = hackerNewsStory.Url,
        Time = DateTimeOffset.FromUnixTimeSeconds(hackerNewsStory.Time),
        Score = hackerNewsStory.Score,
        Title = hackerNewsStory.Title,
        PostedBy = hackerNewsStory.By,
        CommentCount = hackerNewsStory.Descendants
    };
}
