using System.ComponentModel.DataAnnotations;

namespace SantanderTest.Settings;

public sealed class StoryServiceSettings
{
    public const string Section = nameof(StoryServiceSettings);

    [Required, Url]
    public string HackerNewsEndpoint { get; set; } = string.Empty;
}
