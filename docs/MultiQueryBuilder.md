# Multi-Query Builder

A fluent, strongly-typed API for building and executing multi-query requests against the Enterspeed Query API.

## Overview

The Multi-Query Builder allows you to construct multiple queries that will be executed in a single API call. This is particularly useful for:

- Loading related data from different indices in one request
- Optimizing network round-trips
- Ensuring data consistency across multiple queries

## Key Features

✅ **Fluent API** - Chainable methods with IntelliSense support  
✅ **Type-Safe** - Compile-time validation of query structure  
✅ **Immutable Requests** - Built requests are read-only  
✅ **Validation** - Duplicate keys and invalid configurations are caught at build time  
✅ **Flexible** - Support for both fluent builders and pre-constructed queries

## Quick Start

### Basic Usage

```csharp
using Enterspeed.Query.Sdk.Domain.MultiQueriBuilder;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;

var request = new MultiQueryBuilder()
    .AddQuery("users", "user-index", builder => builder
        .WithPagination(0, 10)
        .SortBy("lastName", SortOrder.Asc))
    .AddQuery("products", "product-index", builder => builder
        .WithPagination(0, 20)
        .SortBy("price", SortOrder.Desc))
    .Build();
```

### Using Pre-Constructed Queries

```csharp
var userQuery = new QueryObject
{
    Pagination = new Pagination { Page = 0, PageSize = 10 }
};

var request = new MultiQueryBuilder()
    .AddQuery("users", "user-index", userQuery)
    .Build();
```

### Complex Query Example

```csharp
var request = new MultiQueryBuilder()
    .AddQuery("activeUsers", "user-index", builder => builder
        .WithPagination(0, 25)
        .SortBy("lastName", SortOrder.Asc)
        .SortBy("firstName", SortOrder.Asc)
        .Where(new EqualsOperator { Field = "isActive", Value = true })
        .WithFacet("department", size: 10)
        .WithAliases("summary", "detail"))
    .AddQuery("recentOrders", "order-index", builder => builder
        .WithPagination(0, 50)
        .SortBy("orderDate", SortOrder.Desc)
        .WhereAll(
            new GreaterThanOperator { Field = "orderDate", Value = "2025-01-01" },
            new EqualsOperator { Field = "status", Value = "completed" }))
    .Build();
```

## API Reference

### MultiQueryBuilder

The main builder class for constructing multi-query requests.

#### Methods

##### `AddQuery(string key, string index, Action<IQueryBuilder> builderAction)`

Adds a query using a fluent builder callback.

**Parameters:**
- `key` - Unique identifier for this query (used to retrieve results)
- `index` - The index alias to query against
- `builderAction` - Callback to configure the query

**Returns:** The builder instance for method chaining

**Throws:**
- `ArgumentException` - If key/index is null/empty or key is duplicate
- `InvalidOperationException` - If max query limit (5) is exceeded

##### `AddQuery(string key, string index, QueryObject query)`

Adds a pre-constructed query object.

**Parameters:**
- `key` - Unique identifier for this query
- `index` - The index alias to query against
- `query` - Pre-constructed QueryObject

**Returns:** The builder instance for method chaining

##### `Build()`

Builds an immutable multi-query request.

**Returns:** `MultiQueryRequest` ready for execution

**Throws:**
- `InvalidOperationException` - If no queries have been added

##### `ContainsKey(string key)`

Checks if a query with the specified key exists.

**Returns:** `true` if the key exists; otherwise `false`

##### `Count`

Gets the current number of queries in the builder.

### IQueryBuilder / QueryBuilder

Fluent interface for building individual queries.

#### Methods

##### `WithPagination(int page, int pageSize)`

Sets pagination for the query.

**Parameters:**
- `page` - Page number (zero-based)
- `pageSize` - Number of items per page

**Throws:**
- `ArgumentException` - If page is negative or pageSize < 1

##### `SortBy(string field, SortOrder order = SortOrder.Asc)`

Adds a sort criterion.

**Parameters:**
- `field` - Field name to sort by
- `order` - Sort order (`SortOrder.Asc` or `SortOrder.Desc`)

**Throws:**
- `ArgumentException` - If field is null/empty

##### `Where(FilterOperator filter)`

Adds a single filter to the query.

**Parameters:**
- `filter` - Filter operator to apply

**Throws:**
- `ArgumentNullException` - If filter is null

##### `WhereAll(params FilterOperator[] filters)`

Adds multiple filters with AND logic.

**Parameters:**
- `filters` - Array of filter operators

**Throws:**
- `ArgumentException` - If no filters provided or contains null

##### `WithFacet(string field, string name = null, int size = 10)`

Adds a facet aggregation.

**Parameters:**
- `field` - Field to facet on
- `name` - Optional facet name (defaults to field name)
- `size` - Max facet values to return (default: 10)

**Throws:**
- `ArgumentException` - If field is null/empty or size < 1

##### `WithAliases(params string[] aliases)`

Adds view aliases for different content presentations.

**Parameters:**
- `aliases` - View aliases to include

**Throws:**
- `ArgumentException` - If no aliases provided or contains null/empty

##### `Build()`

Builds the query object.

**Returns:** `QueryObject` with all configured parameters

## Validation & Constraints

### Build-Time Validation

The builder performs validation at build time:

- ✅ **Duplicate Keys** - Rejected with clear error message
- ✅ **Max Query Limit** - Maximum 5 queries per request (API limitation)
- ✅ **Required Fields** - Keys and indices cannot be null/empty
- ✅ **Empty Requests** - At least one query required

### Example Validation

```csharp
var builder = new MultiQueryBuilder();

// ❌ This will throw - duplicate key
builder.AddQuery("users", "index1", q => {});
builder.AddQuery("users", "index2", q => {}); // ArgumentException

// ❌ This will throw - no queries
builder.Build(); // InvalidOperationException

// ❌ This will throw - exceeds max
builder.AddQuery("q1", "index", q => {});
builder.AddQuery("q2", "index", q => {});
builder.AddQuery("q3", "index", q => {});
builder.AddQuery("q4", "index", q => {});
builder.AddQuery("q5", "index", q => {});
builder.AddQuery("q6", "index", q => {}); // InvalidOperationException
```

## Filter Operators

Available filter operators for use with `Where()` and `WhereAll()`:

```csharp
// Equality
new EqualsOperator { Field = "status", Value = "active" }
new NotEqualsOperator { Field = "status", Value = "deleted" }

// Comparison
new GreaterThanOperator { Field = "age", Value = 18 }
new GreaterThanOrEquals { Field = "price", Value = 100 }
new LessThanOperator { Field = "stock", Value = 10 }
new LessThanOrEqualsOperator { Field = "discount", Value = 50 }

// Text
new ContainsOperator { Field = "description", Value = "premium" }

// Collection
new InOperator { Field = "category", Value = new[] { "electronics", "computers" } }
```

## Best Practices

### 1. Use Descriptive Keys

```csharp
// ✅ Good - clear what each query returns
.AddQuery("activeUsers", "user-index", ...)
.AddQuery("featuredProducts", "product-index", ...)

// ❌ Bad - unclear naming
.AddQuery("q1", "user-index", ...)
.AddQuery("data", "product-index", ...)
```

### 2. Keep Queries Focused

```csharp
// ✅ Good - specific, targeted queries
.AddQuery("topSellers", "product-index", b => b
    .WithPagination(0, 10)
    .SortBy("salesCount", SortOrder.Desc))

// ❌ Bad - overly broad, unfocused
.AddQuery("allData", "product-index", b => b
    .WithPagination(0, 1000))
```

### 3. Leverage Fluent Chaining

```csharp
// ✅ Good - clear, readable flow
.AddQuery("users", "user-index", builder => builder
    .WithPagination(0, 10)
    .SortBy("name", SortOrder.Asc)
    .Where(new EqualsOperator { Field = "active", Value = true }))
```

### 4. Reuse Query Objects When Appropriate

```csharp
// Define common query configurations
var standardPagination = new QueryObject
{
    Pagination = new Pagination { Page = 0, PageSize = 10 }
};

var request = new MultiQueryBuilder()
    .AddQuery("users", "user-index", standardPagination)
    .AddQuery("products", "product-index", standardPagination)
    .Build();
```

### 5. Check for Duplicate Keys

```csharp
var builder = new MultiQueryBuilder();

if (!builder.ContainsKey("users"))
{
    builder.AddQuery("users", "user-index", q => {});
}
```

## Immutability

Once built, a `MultiQueryRequest` is immutable:

```csharp
var builder = new MultiQueryBuilder()
    .AddQuery("users", "user-index", q => {});

var request1 = builder.Build(); // 1 query

builder.AddQuery("products", "product-index", q => {});
var request2 = builder.Build(); // 2 queries

// request1 still has only 1 query - immutable!
```

## Testing

The builder includes comprehensive unit tests covering:

- ✅ Query construction with fluent builders
- ✅ Query construction with pre-built objects
- ✅ Duplicate key detection
- ✅ Max query limit enforcement
- ✅ Build-time validation
- ✅ Immutability guarantees
- ✅ API contract serialization

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~MultiQueriBuilder"
```

## Execution (Coming Soon)

The execution layer and response handling will be added in a future update, supporting:

- Single service method for execution
- Partial failure handling
- Strongly-typed response retrieval
- Success/failure response envelopes

## Related Documentation

- [Enterspeed Query API Documentation](https://docs.enterspeed.com/api#tag/Query/operation/queryMultiContentPost)
- Query Response Handling (Coming Soon)
- Error Handling Patterns (Coming Soon)

## Support

For questions or issues, please refer to the [Enterspeed Documentation](https://docs.enterspeed.com) or contact support.
