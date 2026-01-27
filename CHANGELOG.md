# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
  - Fluent Multi-Query Builder API for constructing multi-query requests
  - `MultiQueryBuilder` - Main builder class with duplicate key detection and max query limit validation
  - `QueryBuilder` - Fluent interface for building individual queries with pagination, sorting, filtering, facets, and aliases
  - `MultiQueryRequest` - Immutable container for multi-query requests
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
  - **Fluent Multi-Query Response API** - Strongly-typed result retrieval with `response.Get<T>("queryName")`
  - `ISuccess<T>` and `IFailure` interfaces for type-safe response handling
  - `SuccessResponse<T>` and `FailureResponse<T>` implementations with query result data
  - Response caching per query name and type for performance optimization
  - Helper methods: `ContainsQuery()` and `GetQueryNames()` for query inspection
  - Improved multi-query response architecture with separation between internal deserialization models and public API
  - Enhanced error handling with detailed error messages and error codes (e.g., `QUERY_NOT_FOUND`, `TYPE_MISMATCH`, `NO_RESPONSE`)
  - **IHttpClientFactory Integration** - Standard .NET dependency injection pattern for HttpClient management
  - `Microsoft.Extensions.Http` package dependency for proper HttpClient lifecycle management
  - **Comprehensive Test Suite** - Full test coverage using Verify.Xunit for snapshot testing
  - Unit tests for fluent builder patterns (QueryBuilder, FilterBuilder, MultiQueryBuilder)
  - Unit tests for response handling (SuccessResponse, FailureResponse, MultiQueryApiResponse)
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

## [1.0.0 - 2025-10-05]
### Added
  - Initial release of the Enterspeed Query .NET SDK

## [2.0.0 - 2025-29-12]
### Added 
  - Added support for .NET 10
