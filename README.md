# SantanderTest
A simple implementation of proxy data cache service for HackerNews API in .NET8

## Running
1. Clone repository `git clone https://github.com/miloszskalecki/SantanderTest.git`
2. Execute command `powershell start http://localhost:5000/swagger/index.html | dotnet run --project SantanderTest` from the directory solution is located
3. Alternatively open the solution from Visual Studio and run SantanderTest profile
4. Cache expiration settings can be changed in `appsettings.Development.json`

## Assumptions
1. List of best story ids returned from [Best Stories API](https://hacker-news.firebaseio.com/v0/beststories.json) is ordered by descending score value. Although it is not explicitly stated on the [Hacker News API](https://github.com/HackerNews/API) documentation page, inspection of the data confirms that is in fact the case. 
2. Stories can be edited
3. Handling of the `Dead` flag is not necessary 

## Given more time
1. I would handle `Dead` flag
2. I would chunkify fetching of the individual stories to avoid thread pool starvation on incoming requests
2. There is probably a better implementartion with optimistick locking / lockless / minimal locking via AsyncLazy<T> and ConcurrentDictionary<K,V>
