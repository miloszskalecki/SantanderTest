using RichardSzalay.MockHttp;
using SantanderTest.Clients;
using System.Net.Mime;
using System.Net;
using System.Text.Json;

namespace SantanderTest.Tests.Clients;

internal class HackerNewsApiTests
{
    // this is not necessary as refit framework could be used instead to generate client
    // implementing tests for the sake of full test coverage

    private const string BaseAddress = "http://test/";

    private MockHttpMessageHandler _handler = null!;
    private HackerNewsApi _api = null!;

    [SetUp]
    public void Setup()
    {
        _handler = new();
        _api = new(new HttpClient(_handler) { BaseAddress = new Uri(BaseAddress) });
    }

    [Test]
    public async Task GetBestStoriesAsyncReturnsEmptyList()
    {
        _handler.When(HttpMethod.Get, BaseAddress + "beststories.json")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, "null");

        var bestStories = await _api.GetBestStoriesAsync();

        Assert.That(bestStories, Is.Null);
    }

    [Test]
    public async Task GetBestStoriesAsyncReturnsListOfIds()
    {
        _handler.When(HttpMethod.Get, BaseAddress + "beststories.json")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, "[1,2,3]");

        var bestStories = await _api.GetBestStoriesAsync();

        Assert.That(bestStories, Is.Not.Null);
        Assert.That(bestStories, Is.EqualTo(new long[] { 1, 2, 3 }));
    }

    [Test]
    public async Task GetStoryAsyncReturnsNull()
    {
        _handler.When(HttpMethod.Get, BaseAddress + "item/1.json")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, "null");

        var story = await _api.GetStoryAsync(1);

        Assert.That(story, Is.Null);
    }

    [Test]
    public async Task GetStoryAsyncReturnsStory()
    {
        var expected = new HackerNewsStory { Id = 1 };

        _handler.When(HttpMethod.Get, BaseAddress + "item/1.json")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, JsonSerializer.Serialize(expected));

        var actual = await _api.GetStoryAsync(1);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Id, Is.EqualTo(expected.Id));
    }
}
