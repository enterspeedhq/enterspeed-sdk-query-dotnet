# Example Project: Enterspeed Query .NET SDK

This example project demonstrates advanced and practical usage of the Enterspeed Query .NET SDK, complementing the main [README](../README.md).

## About This Example

The main README covers:
- Installation and service registration
- Basic usage with the fluent and typed `QueryBuilder`
- Available filter operators and simple query patterns

This example project focuses on:
- Advanced query scenarios
- Real-world usage patterns in Web API controllers
- Multi-query requests and direct object construction
- Typed and non-typed query builder usage in API endpoints

## Project Structure
- `Program.cs`: Shows how to register the Enterspeed query service in a .NET application.
- `Controllers/`:
  - `QueriesTypedController.cs`: Examples of using the typed `QueryBuilder` for type-safe, model-driven queries in API endpoints.
  - `QueriesFluentController.cs`: Examples of using the non-typed (fluent) `QueryBuilder` for flexible query construction.
  - `DirectQueryObjectController.cs`: Demonstrates direct object construction for advanced or custom scenarios.
- `Domain/`: Example model classes (e.g., Movie, MovieCredits, Persons) used in typed queries.

## Advanced Usage Examples

### Multi-Query Requests
Execute multiple queries in a single request and handle their results by name:
```csharp
var request = new QueryBuilder()
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
    .Build();

var response = await _enterspeedQueryService.Query(
    "environment-******-****-****-****-**********",
    request);

var activeUsersResult = response.Response.Single(x => x.Name == "activeUsers");
var recentOrdersResult = response.Response.Single(x => x.Name == "recentOrders");
```

### Direct Object Construction
For scenarios requiring full control, you can construct query objects directly:
```csharp
var query = new QueryObject
{
    Sort = new List<Sort> { new Sort { Field = "_updatedAt", Order = SortOrder.Desc } },
    Facets = new List<Facet> { new Facet { Field = "tags", Name = "Tags", Size = 3 } },
    Filters = new AndOperator
    {
        And = new List<IOperator>
        {
            new EqualsOperator<bool> { Field = "isActive", Value = true },
            new OrOperator
            {
                Or = new List<IOperator>
                {
                    new EqualsOperator<bool> { Field = "isGlobal", Value = true },
                    new InOperator<string> { Field = "category", Value = new List<string> { "selected category 1", "selected category 2" } }
                }
            }
        }
    },
    Aliases = new List<string> { "blogPostTile" },
    Pagination = new Pagination { Page = 0, PageSize = 10 }
};

var response = await _enterspeedQueryService.Query("environment-******-****-****-****-**********", "blogIndex", query);
```

### Advanced Logical Grouping
Demonstrates complex filter logic with nested AND/OR groups:
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

## How to Explore Further
- Review `Program.cs` for service registration.
- Explore the controllers in `Controllers/` for hands-on API usage patterns:
  - Typed queries for strong typing and model validation
  - Non-typed queries for flexibility
  - Direct object construction for custom scenarios
- Check the `Domain/` folder for example model classes used in typed queries.

For a full overview of installation, basic usage, and filter operators, see the [main README](../README.md).
