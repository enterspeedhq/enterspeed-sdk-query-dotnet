# [Enterspeed Query .NET SDK](https://www.enterspeed.com/) &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE) [![NuGet version](https://img.shields.io/nuget/v/Enterspeed.Query.Sdk)](https://www.nuget.org/packages/Enterspeed.Query.Sdk/) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/enterspeedhq/enterspeed-sdk-query-dotnet/pulls)

## Installation

With .NET CLI

```bash
dotnet add package Enterspeed.Query.Sdk --version <version>
```

With Package Manager

```bash
Install-Package Enterspeed.Query.Sdk -Version <version>
```
## How to use

### Register services
Service has to be added to the service collection. This can be one by using the following extension method.
```c#
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddEnterspeedQueryService())
    .Build();
```

### Using the Fluent Query Builder

The SDK provides a fluent `QueryBuilder` API for constructing queries in a type-safe, intuitive way. This is the recommended approach for building queries.

#### Basic Query Example

```c#
using Enterspeed.Query.Sdk.Domain.MultiQueriBuilder;

var query = new QueryBuilder()
    .WithPagination(0, 10)
    .SortBy("_updatedAt", SortOrder.Desc)
    .Where(f => f.Equals("isActive", true))
    .Build();

var response = await _enterspeedQueryService.Query(
    "environment-******-****-****-****-**********", 
    "blogIndex", 
    query);
```

#### Lambda-Based Filtering

The fluent API supports lambda-based filters that automatically combine multiple conditions:

```c#
// Single filter
var query = new QueryBuilder()
    .Where(f => f.Equals("status", "active"))
    .Build();

// Multiple conditions (automatically combined with AND)
var query = new QueryBuilder()
    .Where(f => f
        .Equals("status", "active")
        .GreaterThan("age", 18)
        .LessThan("age", 65))
    .Build();

// Multiple Where() calls accumulate with AND
var query = new QueryBuilder()
    .Where(f => f.Equals("status", "active"))
    .Where(f => f.GreaterThan("age", 18))
    .Where(f => f.NotEquals("role", "guest"))
    .Build();
```

#### Available Filter Operators

**Comparison Operators:**
- `.Equals(field, value, caseInsensitive: bool?)` - Equality comparison
- `.NotEquals(field, value, caseInsensitive: bool?)` - Inequality comparison
- `.GreaterThan(field, value)` - Greater than
- `.GreaterThanOrEquals(field, value)` - Greater than or equal
- `.LessThan(field, value)` - Less than
- `.LessThanOrEquals(field, value)` - Less than or equal

**String Operators:**
- `.Contains(field, pattern, caseInsensitive: bool?)` - Pattern matching (use `*` wildcards)

**Collection Operators:**
- `.In(field, ...values)` - Value in list

```c#
// Case-insensitive search
var query = new QueryBuilder()
    .Where(f => f.Equals("title", "hoodie", caseInsensitive: true))
    .Build();

// Pattern matching
var query = new QueryBuilder()
    .Where(f => f.Contains("description", "*premium*"))
    .Build();

// Value in list
var query = new QueryBuilder()
    .Where(f => f.In("category", "electronics", "computers", "phones"))
    .Build();
```

#### Nested Logical Grouping (OR/AND)

Create complex nested filter logic with OR and AND groups:

```c#
// OR group
var query = new QueryBuilder()
    .Where(f => f
        .Equals("title", "hoodie")
        .And().Or(o => o
            .Equals("isInStock", true)
            .Equals("allowPreorder", true)))
    .Build();

// Nested AND group
var query = new QueryBuilder()
    .Where(f => f
        .Equals("status", "active")
        .And(a => a
            .GreaterThanOrEquals("price", 10)
            .LessThanOrEquals("price", 100)))
    .Build();

// Complex nested structure
var query = new QueryBuilder()
    .Where(f => f
        .Equals("status", "active")
        .And().Or(o => o
            .Equals("type", "A")
            .Equals("type", "B"))
        .And().And(a => a
            .GreaterThan("score", 50)
            .LessThan("score", 100)))
    .Build();
```

#### Complete Query with All Features

```c#
var query = new QueryBuilder()
    .WithPagination(0, 25)
    .SortBy("_updatedAt", SortOrder.Desc)
    .SortBy("title", SortOrder.Asc)
    .Where(f => f
        .Equals("isActive", true)
        .And().Or(o => o
            .Equals("isGlobal", true)
            .In("category", "selected category 1", "selected category 2")))
    .WithFacet("tags", name: "Tags", size: 3)
    .WithFacet("category", name: "Category", size: 10)
    .WithAliases("blogPostTile", "blogPostDetail")
    .Build();

var response = await _enterspeedQueryService.Query(
    "environment-******-****-****-****-**********", 
    "blogIndex", 
    query);
```

#### Multi-Query Builder

Execute multiple queries in a single request using the `MultiQueryBuilder`:

```c#
using Enterspeed.Query.Sdk.Domain.MultiQueriBuilder;

var request = new MultiQueryBuilder()
    .AddQuery("activeUsers", "user-index", builder => builder
        .WithPagination(0, 25)
        .SortBy("lastName", SortOrder.Asc)
        .Where(f => f.Equals("isActive", true))
        .WithFacet("role", size: 10)
        .WithAliases("userTile"))
    
    .AddQuery("recentOrders", "order-index", builder => builder
        .WithPagination(0, 50)
        .SortBy("orderDate", SortOrder.Desc)
        .Where(f => f
            .GreaterThan("orderDate", "2025-01-01")
            .Equals("status", "completed")))
    
    .AddQuery("facetsOnly", "product-index", builder => builder
        .WithPagination(0, 0)  // Get facets only, no results
        .WithFacet("category")
        .WithFacet("brand"))
    
    .Build();

var response = await _enterspeedQueryService.Query(
    "environment-******-****-****-****-**********", 
    request);

// Access individual query results by name
var activeUsersResult = response.Response.Single(x => x.Name == "activeUsers");
var recentOrdersResult = response.Response.Single(x => x.Name == "recentOrders");
var facetsResult = response.Response.Single(x => x.Name == "facetsOnly");
```

#### Backward Compatibility: Power-User API

The fluent builder is fully backward compatible with the direct operator API:

```c#
// Direct filter operators (power-user API)
var query = new QueryBuilder()
    .Where(new EqualsOperator<bool> { Field = "isActive", Value = true })
    .WhereAll(
        new GreaterThanOperator<int> { Field = "age", Value = 18 },
        new LessThanOperator<int> { Field = "age", Value = 65 })
    .Build();

// Mix lambda and direct operators
var query = new QueryBuilder()
    .Where(f => f.Equals("status", "active"))
    .Where(new EqualsOperator<bool> { Field = "verified", Value = true })
    .Build();
```

### Alternative: Direct Object Construction

The examples below demonstrate the traditional approach using direct object construction. While this approach is still fully supported, **we recommend using the fluent QueryBuilder API** shown above for better readability and type safety.

```c#
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Extensions;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;

namespace Query;

public class MyQueryService
{
    private readonly IEnterspeedQueryService _enterspeedQueryService;

    public MyQueryService(IEnterspeedQueryService enterspeedQueryService)
    {
        _enterspeedQueryService = enterspeedQueryService;
    }

    // Query an Enterspeed index 
    public async Task<QueryApiResponse> Query()
    {
        var query = new QueryObject
        {
            Sort = new List<Sort>
            {
                new Sort()
                {
                    Field = "_updatedAt",
                    Order = SortOrder.Desc
                }
            },
            Facets = new List<Facet>
            {
                new Facet()
                {
                    Field = "tags",
                    Name = "Tags",
                    Size = 3
                }
            },
            Filters = new AndOperator
            {
                And = new List<IOperator>
                {
                    new EqualsOperator<bool>
                    {
                        Field = "isActive",
                        Value = true
                    },
                    new OrOperator
                    {
                        Or = new List<IOperator>
                        {
                            new EqualsOperator<bool>
                            {
                                Field = "isGlobal",
                                Value = true
                            },
                            new InOperator<string>
                            {
                                Field = "category",
                                Value = new List<string> { "selected category 1", "selected category 2" }
                            }
                        }
                    }
                }
            },
            Aliases = new List<string> { "blogPostTile" },
            Pagination = new Pagination
            {
                Page = 0,
                PageSize = 10
            }
        };

        var response = await _enterspeedQueryService.Query("environment-******-****-****-****-**********", "blogIndex", query);

        return response;
    }

    // Query an Enterspeed index and get strongly typed result
    public async Task<List<BlogPost>> QueryTyped()
    {
        var typedResult = await _enterspeedQueryService.QueryTyped("environment-******-****-****-****-**********", "blogIndex", new QueryObject());

        List<BlogPost> blogPosts = typedResult.Response.Results.GetContent<BlogPost>();

        return blogPosts;
    }

    // Multiple queries against an Enterspeed index 
    public async Task<MultiQueryApiResponse> MultiQuery()
    {
        var multiQuery = new List<MultiQueryObject>{
            new()
            {
                Index = "blogIndex",
                Name = "blogPosts",
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new EqualsOperator<string>
                        {
                            Field = "title",
                            Value = "My Blog Post",
                            CaseInsensitive = true
                        }
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            },
            new()
            {
                Index = "blogIndex",
                Name = "allCategoryFacets",
                Facets = new List<Facet>
                {
                    new()
                    {
                        Field = "category",
                        Name = "Category"
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 0
                }
            }
        };

        var response = await _enterspeedQueryService.Query("environment-******-****-****-****-**********", multiQuery);

        var blogPostsQueryResponse = response.Response.Single(x => x.Name == "blogPosts");
        var allFacetsQueryResponse = response.Response.Single(x => x.Name == "allFacets");

        return response;
    }

    // Multiple queries against an Enterspeed index and get strongly typed results
    public async Task<MultiQueryApiResponse<IContent>> MultiQueryTyped()
    {
        var multiQuery = new List<MultiQueryObject>{
            new()
            {
                Index = "blogIndex",
                Name = "blogPosts",
                Sort = new List<Sort>
                {
                    new()
                    {
                        Field = "_updatedAt",
                        Order = SortOrder.Desc
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            },
            new()
            {
                Index = "productIndex",
                Name = "products",
                Sort = new List<Sort>
                {
                    new()
                    {
                        Field = "_updatedAt",
                        Order = SortOrder.Desc
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            }
        };

        var response = await _enterspeedQueryService.QueryTyped("environment-******-****-****-****-**********", multiQuery);

        List<BlogPost> blogPosts = response.Response.Single(x => x.Name == "blogPosts").Results.GetContent<BlogPost>();
        List<Product> products = response.Response.Single(x => x.Name == "allFacets").Results.GetContent<Product>();

        return response;
    }

    public class BlogPost
    {
        [JsonPropertyName("date")]
        public DateTime PublishedDate { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string[] Tags { get; set; }
    }

    public class Product
    {
        public string Url { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public string Sku { get; set; }
        public string Category { get; set; }
    }
}
```


## Contributing

Pull requests are very welcome.  
Please fork this repository and make a PR when you are ready.  

Otherwise you are welcome to open an Issue in our [issue tracker](https://github.com/enterspeedhq/enterspeed-sdk-query-dotnet/issues).

## License

Enterspeed .NET SDK is [MIT licensed](./LICENSE)
