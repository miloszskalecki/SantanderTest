using System.ComponentModel.DataAnnotations;

namespace SantanderTest.Settings;

public sealed class StoryServiceSettings
{
    public const string Section = nameof(StoryServiceSettings);

    [Required]
    public TimeSpan StoryExpiration { get; set; }

    [Required]
    public TimeSpan StoryListExpiration { get; set; }

    [Required, Url]
    public string HackerNewsEndpoint { get; set; } = string.Empty;
}
