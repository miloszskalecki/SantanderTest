using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SantanderTest.Clients;
using SantanderTest.Services;
using SantanderTest.Settings;

namespace SantanderTest.Tests.Services;

public class StoryServiceTests
{
    private Mock<IMemoryCache> _cacheMock;
    private Mock<IHackerNewsClient> _clientMock;
    private Mock<ILogger<StoryService>> _logger;
    private Mock<IOptions<StoryServiceSettings>> _options;
    private StoryService _service;
    private StoryServiceSettings _settings;

    [SetUp]
    public void Setup()
    {
        // please note that the service code uses mem cache extensions 
        // therefore we need to mock the actual method used - createEntry
        // also memcache.TryGetValue moq matcher requires object? to work properly

        _settings = new();

        _logger = new Mock<ILogger<StoryService>>();
        _options = new Mock<IOptions<StoryServiceSettings>>();
        _options.SetupGet(p => p.Value).Returns(_settings);

        _clientMock = new();
        _cacheMock = new();
        _service = new(
            _cacheMock.Object,
            _clientMock.Object,
            _logger.Object,
            _options.Object);
    }

    [Test]
    public async Task GetBestStoriesAsyncReturnsEmptyIfCountIsZeroOrNegative()
    {
        var stories1 = await _service.GetBestStoriesAsync(0);
        var stories2 = await _service.GetBestStoriesAsync(-1);

        Assert.That(stories1, Is.Empty);
        Assert.That(stories2, Is.Empty);
    }

    [Test]
    public async Task GetBestStoriesAsyncFetchesAndCachesListIfNotYetCached()
    {
        object? notCached = null;

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out notCached))
            .Returns(false);

        _cacheMock.Setup(p => p.CreateEntry(StoryService.BestStoriesKey))
            .Returns(Mock.Of<ICacheEntry>())
            .Verifiable(Times.Once());

        _clientMock.Setup(p => p.GetBestStoriesAsync())
            .Returns(Task.FromResult<List<long>?>(null))
            .Verifiable(Times.Once());

        var stories = await _service.GetBestStoriesAsync(10);

        _clientMock.VerifyAll();
        _cacheMock.VerifyAll();

        Assert.That(stories, Is.Empty);
    }

    [Test]
    public async Task GetBestStoriesAsyncFetchesAndCachesListIfNotYetCached1()
    {
        object? notCached = null;

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out notCached))
            .Returns(false);

        _cacheMock.Setup(p => p.CreateEntry(StoryService.BestStoriesKey))
            .Returns(Mock.Of<ICacheEntry>())
            .Verifiable(Times.Once());

        _clientMock.Setup(p => p.GetBestStoriesAsync())
            .Returns(Task.FromResult<List<long>?>([]))
            .Verifiable(Times.Once());

        var stories = await _service.GetBestStoriesAsync(10);

        _clientMock.VerifyAll();
        _cacheMock.VerifyAll();

        Assert.That(stories, Is.Empty);
    }

    [Test]
    public async Task GetBestStoriesAsyncDoesNotFetchListIfCached()
    {
        object? storyListTask = Task.FromResult<List<long>?>([]);

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out storyListTask))
            .Returns(true);

        _ = await _service.GetBestStoriesAsync(10);

        _clientMock.Verify(p => p.GetBestStoriesAsync(), Times.Never());
    }

    [Test]
    public async Task GetBestStoriesAsyncDoesNotFetchStoryIfCached()
    {
        const long bestStoryId = 23;

        object? storyTask = Task.FromResult(new Story());
        object? storyListTask = Task.FromResult<List<long>?>([bestStoryId]);

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out storyListTask))
            .Returns(true);

        _cacheMock.Setup(p => p.TryGetValue(It.Is<long>(sid => sid == bestStoryId), out storyTask))
            .Returns(true);

        _ = await _service.GetBestStoriesAsync(10);

        _clientMock.Verify(p => p.GetStoryAsync(It.IsAny<long>()), Times.Never());
    }

    [Test]
    public async Task GetBestStoriesAsyncFetchesAndCachesStoryIfNotYetCached()
    {
        const long storyId = 23;

        object? storyListTask = Task.FromResult<List<long>?>([storyId]);

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out storyListTask))
            .Returns(true);

        _cacheMock.Setup(p => p.CreateEntry(It.Is<long>(sid => sid == storyId)))
            .Returns(Mock.Of<ICacheEntry>())
            .Verifiable(Times.Once());

        _clientMock.Setup(p => p.GetStoryAsync(It.IsAny<long>()))
            .Returns(Task.FromResult<HackerNewsStory?>(new() { By = "by"}))
            .Verifiable(Times.Once());

        var stories1 = await _service.GetBestStoriesAsync(10);

        _clientMock.VerifyAll();
        _cacheMock.VerifyAll();

        Assert.That(stories1, Is.Not.Empty);
    }

    [Test]
    public async Task GetBestStoriesAsyncMappsReceivedStories()
    {
        var hackerStory = new HackerNewsStory
        {
            By = "by",
            Url = "url",
        };

        _cacheMock.Setup(p => p.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _clientMock.Setup(p => p.GetBestStoriesAsync())
            .Returns(Task.FromResult<List<long>?>([1L]));

        _clientMock.Setup(p => p.GetStoryAsync(It.IsAny<long>()))
            .Returns(Task.FromResult<HackerNewsStory?>(hackerStory));

        var stories = await _service.GetBestStoriesAsync(10);
        var story = stories.First();

        Assert.That(story.PostedBy, Is.EqualTo(hackerStory.By));
        Assert.That(story.Uri, Is.EqualTo(hackerStory.Url));
    }

    [Test]
    public async Task GetBestStoriesAsyncFetchesListAndStoryOnceForMultipleCallers()
    {
        const long storyId = 123;

        _settings.StoryExpiration = TimeSpan.FromMinutes(10);
        _settings.StoryListExpiration = TimeSpan.FromMinutes(10);

        var options = new OptionsWrapper<MemoryCacheOptions>(new());

        // in this test, we want to use memorycache implementation 
        // instead of mock to test locking logic
        var service = new StoryService(new MemoryCache(options),
            _clientMock.Object, _logger.Object, _options.Object);

        _clientMock.Setup(p => p.GetBestStoriesAsync())
            .Returns(Task.FromResult<List<long>?>([storyId]))
            .Verifiable(Times.Once());

        _clientMock.Setup(p => p.GetStoryAsync(storyId))
            .Returns(Task.FromResult<HackerNewsStory?>(new()))
            .Verifiable(Times.Once());

        await Task.WhenAll(
            Task.Run(async () => await service.GetBestStoriesAsync(10)),
            Task.Run(async () => await service.GetBestStoriesAsync(10)),
            Task.Run(async () => await service.GetBestStoriesAsync(10)));

        _clientMock.VerifyAll();
    }

    [Test]
    public async Task GetBestStoriesAsyncFetchesOnlyCountOfStories()
    {
        object? storyTask1 = Task.FromResult(new Story());
        object? storyTask2 = Task.FromResult(new Story());
        object? storyListTask = Task.FromResult<List<long>?>([1L, 2L]);

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out storyListTask))
            .Returns(true);

        _cacheMock.Setup(p => p.TryGetValue(It.Is<long>(sid => sid == 1L), out storyTask1))
            .Returns(true);

        _cacheMock.Setup(p => p.TryGetValue(It.Is<long>(sid => sid == 2L), out storyTask2))
            .Returns(true);

        var stories1 = await _service.GetBestStoriesAsync(1);
        var stories2 = await _service.GetBestStoriesAsync(2);

        Assert.That(stories1.Count(), Is.EqualTo(1));
        Assert.That(stories2.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetBestStoriesAsyncReturnsOnlyExistingStories()
    {
        object? storyTask1 = Task.FromResult(new Story());
        object? storyTask2 = Task.FromResult((Story?)null);
        object? storyListTask = Task.FromResult<List<long>?>([1L, 2L]);

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out storyListTask))
            .Returns(true);

        _cacheMock.Setup(p => p.TryGetValue(It.Is<long>(sid => sid == 1L), out storyTask1))
            .Returns(true);

        _cacheMock.Setup(p => p.TryGetValue(It.Is<long>(sid => sid == 2L), out storyTask2))
            .Returns(true);

        var stories = await _service.GetBestStoriesAsync(10);

        Assert.That(stories.Count(), Is.EqualTo(1));
    }

    [Test]
    public void GetBestStoriesAsyncRemovesFailedTasksImmediatelly()
    {
        object? notCached = null;

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out notCached))
            .Returns(false);

        _cacheMock.Setup(p => p.CreateEntry(StoryService.BestStoriesKey))
            .Returns(Mock.Of<ICacheEntry>());

        _clientMock.Setup(p => p.GetBestStoriesAsync())
            .Returns(Task.FromException<List<long>?>(new Exception()));

        var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetBestStoriesAsync(10));

        Assert.That(exception, Is.Not.Null);

        _cacheMock.Verify(p => p.Remove(StoryService.BestStoriesKey));
    }

    [Test]
    public async Task GetBestStoriesLogsTheFetches()
    {
        object? storyTask = null;
        object? storyListTask = null;

        _cacheMock.Setup(p => p.TryGetValue(StoryService.BestStoriesKey, out storyListTask))
            .Returns(false);

        _cacheMock.Setup(p => p.TryGetValue(It.IsAny<long>(), out storyTask))
            .Returns(false);

        _cacheMock.Setup(p => p.CreateEntry(StoryService.BestStoriesKey))
            .Returns(Mock.Of<ICacheEntry>());

        _cacheMock.Setup(p => p.CreateEntry(It.IsAny<long>()))
            .Returns(Mock.Of<ICacheEntry>());

        _clientMock.Setup(p => p.GetBestStoriesAsync())
            .Returns(Task.FromResult<List<long>?>([1L]));

        _clientMock.Setup(p => p.GetStoryAsync(It.IsAny<long>()))
            .Returns(Task.FromResult<HackerNewsStory?>(new()));

        //_logger.Setup(p => p.IsEnabled(It.Is<LogLevel>(a => a == LogLevel.Information)))
        //    .Returns(true)
        //    .Verifiable(Times.Exactly(2));

        _logger.Setup(p => p.Log(
                
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable(Times.Exactly(2));    

        _ = await _service.GetBestStoriesAsync(10);

        _logger.VerifyAll();
    }
}