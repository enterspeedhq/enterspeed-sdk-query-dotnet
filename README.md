# [Enterspeed Query .NET SDK](https://www.enterspeed.com/) &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE) [![NuGet version](https://img.shields.io/nuget/v/Enterspeed.Query.Sdk)](https://www.nuget.org/packages/Enterspeed.Query.Sdk/) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/enterspeedhq/enterspeed-sdk-query-dotnet/pulls)
The Enterspeed Query .NET SDK empowers .NET developers to query and manage data from Enterspeed indexes with a modern, type-safe, and fluent API. It supports both simple and advanced scenarios, making it easy to integrate Enterspeed into your applications.

**Key Features:**
- Typed query builders for model-driven, type-safe queries
- Fluent API for intuitive query construction
- Advanced filtering, sorting, pagination, and facets
- Strongly typed responses for reliable data handling
- Easy integration with .NET dependency injection

## Installation

With .NET CLI:
```bash
dotnet add package Enterspeed.Query.Sdk --version <version>
```
With Package Manager:
```bash
Install-Package Enterspeed.Query.Sdk -Version <version>
```

## Getting Started

### Register Services
Add the Enterspeed query service to your DI container:
```csharp
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddEnterspeedQueryService())
    .Build();
```

## Using the Fluent Query Builders

The SDK provides a fluent `QueryBuilder` API for constructing queries in a type-safe, intuitive way. When using generics,
filters, sorting, and facets are aligned with your model’s properties—property names are automatically camelCased (e.g.,
IsActive becomes isActive), unless a `[JsonPropertyName]` attribute is present, in which case its value is used. A
non-generic approach is also available if you prefer not to use strong typing.

**Example: Typed Query Builder (Full Flow)**
```csharp
var request = new QueryBuilder()
    .AddQuery<Movie>("key", "indexName", builder => builder 
      .WithPagination(0, 25)
      .SortBy(x => x.UpdatedAt, SortOrder.Desc)
      .Where(f => f
          .Equals(x => x.IsActive, true)
          .Or(o => o
              .Equals(x => x.IsGlobal, true)
              .In(x => x.Category, "selected category 1", "selected category 2"))
      )
      .WithFacet("tags", name: "Tags", size: 3)
      .WithFacet("category", name: "Category", size: 10)
      .WithAliases("blogPostTile", "blogPostDetail"))
    .Build();

var response = await _enterspeedQueryService.Query(
    "environment-******-****-****-****-**********",
    request,
    CancellationToken.None);

var movieList = response.Get<Movie>("key");

if (movieList is ISuccess<List<Movie>> success)
{
    success.Result.ForEach(movie =>
    {
        Console.WriteLine($"Title: {movie.Title}, Release Date: {movie.ReleaseDate}");
    });
}

if (movieList is IFailure failure)
{
    Console.WriteLine($"Query failed: {failure.ErrorMessage}");
}

public record Movie
{
    [JsonPropertyName("_updatedAt")]
    public DateTime UpdatedAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsGlobal { get; init; }
    public string Category { get; init; }
    public string Title { get; init; }
    public DateTime ReleaseDate { get; init; }
}
```

See the [Example project](./Example/README.md) for more advanced usage and scenarios, including Web API integration and direct object construction.

## Using the Non-Typed Fluent Query Builder

The non-generic `QueryBuilder` allows you to build queries without strong typing:

```csharp
var query = new QueryBuilder()
    .AddQuery("key", "indexName", builder => builder
      .WithPagination(0, 10)
      .SortBy("_updatedAt", SortOrder.Desc)
      .Where(f => f.Equals("isActive", true)))
    .Build();

var response = await _enterspeedQueryService.Query(
    "environment-******-****-****-****-**********", 
    "blogIndex", 
    query);
```

## Lambda-Based Filtering

The fluent API supports lambda-based filters that automatically combine multiple conditions. Multiple `Where()` calls are implicitly ANDed together, and the first filter is always an AND group by default.

```csharp
// Single filter
var query = new QueryBuilder()
    .AddQuery("key", "indexName", builder => builder
        .Where(f => f.Equals("status", "active")))
    .Build();

// Multiple conditions (AND)
var query = new QueryBuilder()
    .Where(f => f
        .Equals("status", "active")
        .GreaterThan("age", 18)
        .LessThan("age", 65))
    .Build();
```

## Available Filter Operators

**Comparison Operators:**
- `.Equals(field, value, caseInsensitive: bool = false)` - Equality comparison
- `.NotEquals(field, value, caseInsensitive: bool = false)` - Inequality comparison
- `.GreaterThan(field, value)` - Greater than
- `.GreaterThanOrEquals(field, value)` - Greater than or equal
- `.LessThan(field, value)` - Less than
- `.LessThanOrEquals(field, value)` - Less than or equal

**String Operators:**
- `.Contains(field, pattern, caseInsensitive: bool = false)` - Pattern matching (use `*` wildcards)

**Collection Operators:**
- `.In(field, ...values)` - Value in list

**Examples:**
```csharp
// Case-insensitive search
var query = new QueryBuilder()
    .AddQuery("key", "indexName", builder => builder
    .Where(f => f.Equals("title", "hoodie", caseInsensitive: true)))
    .Build();

// Pattern matching
var query = new QueryBuilder()
    .AddQuery("key", "indexName", builder => builder
    .Where(f => f.Contains("description", "*premium*")))
    .Build();

// Value in list
var query = new QueryBuilder()
    .AddQuery("key", "indexName", builder => builder
    .Where(f => f.In("category", "electronics", "computers", "phones")))
    .Build();

// Typed usage
var query = new QueryBuilder()
    .AddQuery<Movie>("key", "indexName", builder => builder
        .Where(f => f.In(x => x.Category, "electronics", "computers", "phones")))
    .Build();
```

## Nested Logical Grouping (OR/AND)

Create complex nested filter logic with OR and AND groups. Multiple `Where()` calls are implicitly ANDed.

```csharp
var query = new QueryBuilder()
    .Where(f => f
        .Equals("status", "active")
        .Or(o => o
            .Equals("type", "A")
            .Equals("type", "B"))
        .And(a => a
            .GreaterThan("score", 50)
            .LessThan("score", 100)))
    .Build();
```

For more advanced scenarios, such as multi-query, direct object construction, and advanced logical grouping, see the [Example project README](./Example/README.md).

## Contributing

Pull requests are very welcome.  
Please fork this repository and make a PR when you are ready.  

Otherwise you are welcome to open an Issue in our [issue tracker](https://github.com/enterspeedhq/enterspeed-sdk-query-dotnet/issues).

## License

Enterspeed .NET SDK is [MIT licensed](./LICENSE)
