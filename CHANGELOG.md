# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.1]
### Added
  - `ForbiddenApiResponse` — new model to deserialise the flat `{ "error": "Forbidden" }` body returned on HTTP 403
  - `IErrorExtensions.IsForbidden()` — extension method on `IError` to distinguish scope-denied items in a `QueryMultiple` batch from other errors (non-breaking, no interface change)

### Fixed
  - HTTP 403 responses now correctly surface as `QueryApiResponse` with `StatusCode = Forbidden` and the error message populated, instead of falling through to a generic deserialization error with the wrong status code

## [2.0.0]
### Added
  - Fluent Query Builder API for constructing multi-query requests
  - `QueryBuilder` - Main builder class with duplicate key detection and max query limit validation
  - `Query` - Fluent interface for building individual queries with pagination, sorting, filtering, facets, and aliases
  - `QueryRequest` - Immutable container for multi-query requests
  - **Lambda-based Filter Builder API** - Fluent filter construction with `Where(f => f.Equals(...).Or(...)...)`
  - `IFilterBuilder` - Interface for building filter conditions with nested AND/OR groups
  - `FilterBuilder` - Internal implementation that builds filter operator trees matching Enterspeed Query API structure
  - Support for all comparison operators: Equals, NotEquals, GreaterThan, GreaterThanOrEquals, LessThan, LessThanOrEquals
  - Support for string operators: Contains (with case-insensitive option)
  - Support for collection operators: In
  - Support for nested logical grouping: `.Or(o => ...)`, `.And(a => ...)`
  - Multiple `Where()` calls accumulate with implicit AND logic
  - Filters at the top level are implicitly ANDed together (no need for cosmetic `.And()` separator)
  - Case-insensitive comparison support for Equals, NotEquals, and Contains operators
  - **Fluent Query Response API** - Strongly-typed result retrieval with `response.Get<T>("queryName")`
  - `ISuccess<T>` and `IFailure` interfaces for type-safe response handling
  - `SuccessResponse<T>` and `FailureResponse<T>` implementations with query result data
  - Response caching per query name and type for performance optimization
  - Helper methods: `ContainsQuery()` and `GetQueryNames()` for query inspection
  - Fix for deserialization issues with multi-query responses - now properly handles both success and failure cases with correct type mapping
  - **IHttpClientFactory Integration** - Standard .NET dependency injection pattern for HttpClient management
  - `Microsoft.Extensions.Http` package dependency for proper HttpClient lifecycle management
  - **Comprehensive Test Suite** - Full test coverage using Verify.Xunit for snapshot testing
  - Unit tests for fluent builder patterns (QueryBuilder, FilterBuilder, QueryBuilder)
  - Unit tests for response handling (SuccessResponse, FailureResponse, QueryApiResponse)
  - End-to-end integration tests validating complete flow from query building to typed result retrieval
  - Snapshot-based tests for structural validation of builder outputs and response parsing
  - `Verify.Xunit` package integration for maintainable snapshot testing

### Changed
  - **BREAKING**: `EnterspeedQueryConnection` now requires `IHttpClientFactory` via constructor injection
  - Removed manual HttpClient creation and disposal - now managed by `IHttpClientFactory`
  - `AddEnterspeedQueryService()` now configures named HttpClient with base URL, headers, and timeouts
  - Connection pooling and socket management now handled automatically by the framework
  - Simplified `EnterspeedQueryConnection` - removed connection timeout tracking and re-connection logic
  - `Flush()` method is now a no-op as HttpClient lifecycle is managed by the factory
  - Remove single Query interface and single QueryTyped interfaces - now only multi-query support with `QueryBuilder`
## [1.0.0 - 2025-10-05]
### Added
  - Initial release of the Enterspeed Query .NET SDK

## [2.0.0 - 2025-29-12]
### Added 
  - Added support for .NET 10
