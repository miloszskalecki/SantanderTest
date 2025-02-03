using SantanderTest.Clients;
using SantanderTest.Services;

namespace SantanderTest.Tests.Services;

internal class StoryMapperTests
{
    [Test]
    public void ToStoryMapsProperties()
    {
        var hackerStory = new HackerNewsStory
        {
            Id = 123,
            Score = 456,
            Descendants = 789,
            Dead = true,
            By = "by",
            Url = "url",
            Title = "title",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        var story = hackerStory.ToStory();

        Assert.That(story.Title, Is.EqualTo(hackerStory.Title));
        Assert.That(story.Uri, Is.EqualTo(hackerStory.Url));
        Assert.That(story.PostedBy, Is.EqualTo(hackerStory.By));
        Assert.That(story.Score, Is.EqualTo(hackerStory.Score));
        Assert.That(story.CommentCount, Is.EqualTo(hackerStory.Descendants));
        Assert.That(story.Time, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(hackerStory.Time)));
    }
}
