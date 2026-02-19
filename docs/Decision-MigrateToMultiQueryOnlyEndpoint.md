# Decision Document: Migration to Multi-Query-Only Endpoint

| Field           | Value                      |
|-----------------|----------------------------|
| Status          | In Progress                |
| Created Date    | 2026-02-05                 |
| Reviewed Date   | YYYY-MM-DD (if applicable) |
| Owner           | Bjarke                     |
| Contributors    | Brian, Alex                |

---

## 1. 🧭 Purpose

The purpose of this document is to record the decision to migrate the Enterspeed Query API to a Multi-Query-Only endpoint, deprecating and removing the legacy single query endpoints (Single Query and Single Query Typed). This aims to simplify the API surface, reduce maintenance overhead, and unify query handling for all clients. The document answers why this migration is necessary, what alternatives were considered, and how the change impacts the platform.

## 2. 🧱 Scope

This decision covers:
- The deprecation and removal of the single query endpoints (Single Query and Single Query Typed) from the Enterspeed Query API.
- The consolidation of all query operations into the Multi-Query endpoint.
- Updates to SDKs, documentation, and client integrations to support only the Multi-Query endpoint.
- A phased deprecation strategy: first deprecating single query support in the SDK, then fully removing the single query APIs from the backend.

Out of scope:
- Changes to unrelated API endpoints.
- Broader architectural changes outside the Query API.
- Migration strategies for legacy clients (addressed in separate migration guides).

## 3. 🔍 Background & Context

Historically, the Enterspeed Query API supported three query endpoints: Single Query, Single Query Typed, and Multi-Query. While these endpoints share similar underlying implementations, this redundancy led to increased maintenance burden, inconsistent client experiences, and confusion over which endpoint to use for different scenarios. The Multi-Query endpoint offers greater flexibility, supports batching, and aligns with modern API best practices. Previous design decisions and feedback from internal and external stakeholders highlighted the need to simplify the API surface and reduce technical debt by consolidating to a single, unified approach.

## 4. 🛠 Approaches Considered / Explored

| Approach                                      | Pros                                                      | Cons                                                      |
|-----------------------------------------------|-----------------------------------------------------------|-----------------------------------------------------------|
| 1. Maintain both single query and Multi-Query endpoints | Backward compatibility; minimal disruption                 | Increased maintenance; user confusion; technical debt      |
| 2. Immediate removal of single query endpoints | Fastest simplification; no dual maintenance                | High risk of breaking clients; abrupt migration            |
| 3. Phased deprecation: SDK first, then API (Chosen) | Smooth migration; clear communication; reduced risk        | Requires coordination; slightly longer timeline            |

**Decision:**

A phased deprecation approach is chosen as a single, unified task:
- **Step 1:** Deprecate and remove support for the single query APIs in the SDK. New SDK versions will only support the Multi-Query endpoint, signaling to clients that migration is required.
- **Step 2:** After a defined transition period, fully deprecate and remove the single query APIs from the backend.

**Rationale for Phased Deprecation:**
- Provides a clear migration path for clients, allowing them to update their integrations at their own pace before the backend change.
- Reduces risk of service disruption, as issues can be identified and resolved during the SDK transition phase.
- Enables better communication and support for clients, with clear timelines and expectations.
- Treats the deprecation as a single, coordinated strategy rather than two unrelated tasks, ensuring alignment across teams and stakeholders.

### Technical API Consistency Issues

The current 3 endpoints (Single Query, Single Query Typed, and Multi-Query) all share similar underlying implementations for query operations. However, the multi-query endpoint has a critical flaw: it fails completely if any single query in the batch encounters an error, resulting in JSON serialization issues. While investigating and fixing this issue, the team recognized that the multi-query endpoint offers a more consistent and flexible approach to querying data, as it can handle both individual and batch queries with a unified response structure. In contrast, the single query endpoints have significant inconsistencies in their API design, which creates confusion for users and increases the learning curve.

Each endpoint has a different way of accessing query results, with varying patterns for generic type usage and response handling. This inconsistency leads to cognitive load for developers who must learn and remember multiple patterns for essentially the same operation (querying data).
The current dual-endpoint approach creates significant inconsistencies in the SDK interface that confuse users and increase the learning curve:

**Single Query API Pattern:**
```csharp
Task<QueryApiResponse<T>> QueryTyped<T>(string apiKey, string index, QueryObject query, ...)
```
- Returns `QueryApiResponse<T>` with a direct property: `IResponse<T> Response`
- Users access results immediately: `response.Response` (which is `ISuccess<T>` or `IFailure`)
- Direct property access pattern

**Multi Query API Pattern:**
```csharp
Task<MultiQueryApiResponse> Query(string apiKey, List<MultiQueryObject> queries, ...)
```
- Returns `MultiQueryApiResponse` with no direct generic parameter
- Users must call a method: `Get<T>(queryName)` which returns `IResponse<T>`
- Method-based retrieval pattern with query name lookup

**Key Inconsistencies:**
1. **Different Access Patterns**: Single query uses property access (`response.Response`), while multi-query uses method calls (`response.Get<T>("queryName")`).
2. **Generic Type Placement**: Single query has the generic type at the response level (`QueryApiResponse<T>`), while multi-query defers it to the `Get<T>()` method.
3. **Return Type Mismatch**: Although both ultimately return `IResponse<T>`, the path to access it differs significantly.
4. **Cognitive Load**: Users must learn and remember two different patterns for essentially the same operation (querying data).

**Benefits of Single Unified Pattern (Multi-Query Only):**
- **Consistency**: One response pattern across all query operations.
- **Flexibility**: Multi-query naturally handles both single queries (one item in the list) and batch queries.
- **Clearer Semantics**: Named queries make it explicit what data is being retrieved, even for single queries.
- **Reduced API Surface**: Fewer methods and types to maintain, document, and test.
- **Better Discoverability**: Users learn one pattern that works for all scenarios.

By deprecating the single query endpoint and standardizing on multi-query, we eliminate this confusion and provide a cleaner, more intuitive developer experience.

## 5. 🔐 Security Assessment

**Data classification and flow:**
- No sensitive data exposure changes; queries and responses remain as before.
- Multi-Query endpoint supports batching, but enforces the same data access controls as the single query endpoints.

**Access control implications:**
- No changes to authentication or authorization mechanisms.
- All existing access controls are enforced at the Multi-Query endpoint.

**Potential vulnerabilities introduced:**
- Risk of improper batching logic leading to data leakage if not correctly implemented.
- Increased payload size could expose denial-of-service (DoS) vectors if not rate-limited.

**Regulatory/Compliance impact:**
- No new compliance risks identified; data handling remains compliant with existing policies.

**Mitigation strategies and recommendations:**
- Ensure robust input validation and output filtering on the Multi-Query endpoint.
- Monitor and rate-limit large batch requests to prevent abuse.
- Conduct security review and regression testing before deprecating the single query endpoints.

---

*This document will be updated as the migration progresses and reviewed prior to finalization.*
