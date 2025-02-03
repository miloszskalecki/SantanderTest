namespace SantanderTest.Clients;

sealed class HackerNewsStory
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string By { get; init; } = string.Empty;
    public bool Dead { get; init; }
    public long Time { get; init; }
    public int Score { get; init; }
    public int Descendants { get; init; }
}
