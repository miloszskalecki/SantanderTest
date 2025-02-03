using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using SantanderTest.Clients;
using SantanderTest.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOptions<StoryServiceSettings>()
    .BindConfiguration(StoryServiceSettings.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IHackerNewsApi, HackerNewsApi>();
builder.Services.AddHttpClient<IHackerNewsApi, HackerNewsApi>((services, client) =>
{
    var settings = services.GetRequiredService<IOptions<StoryServiceSettings>>();
    client.BaseAddress = new Uri(settings.Value.HackerNewsEndpoint);
});

builder.Services.AddLogging(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.ColorBehavior = LoggerColorBehavior.Enabled;
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fffff] ";
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnableTryItOutByDefault());
}

app.MapGet("/stories/{count}", async (IHackerNewsApi api, int count) =>
{
    var stories = await api.GetBestStoriesAsync();

    return Results.Ok(stories);
})
.WithName("GetBestStories")
.WithDescription("Retrieves the details of the best n stories from the Hacker News API, as determined by their score")
.WithSummary("Retreives best Hacker News stories")
.Produces<IEnumerable<long>>()
.WithOpenApi();

app.Run();