using System;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;

namespace Enterspeed.Query.Sdk.Examples
{
    /// <summary>
    /// Example usage of the MultiQueryBuilder for constructing multi-query requests.
    /// This demonstrates the fluent API patterns for building queries.
    /// </summary>
    public class MultiQueryBuilderExamples
    {
        /// <summary>
        /// Basic example: Two simple queries with pagination
        /// </summary>
        public MultiQueryRequest BasicExample()
        {
            var request = new MultiQueryBuilder()
                .AddQuery("users", "user-index", builder => builder
                    .WithPagination(0, 10))
                .AddQuery("products", "product-index", builder => builder
                    .WithPagination(0, 20))
                .Build();

            return request;
        }

        /// <summary>
        /// Lambda-based filter example: Using the new fluent filter API
        /// </summary>
        public MultiQueryRequest LambdaFilterExample()
        {
            var request = new MultiQueryBuilder()
                .AddQuery("activeUsers", "user-index", builder => builder
                    .WithPagination(0, 25)
                    .Where(f => f
                        .Equals("status", "active")
                        .GreaterThan("age", 18))
                    .SortBy("lastName", SortOrder.Asc))
                .Build();

            return request;
        }

        /// <summary>
        /// Nested OR example: Complex filter with nested OR groups
        /// </summary>
        public MultiQueryRequest NestedOrExample()
        {
            var request = new MultiQueryBuilder()
                .AddQuery("availableProducts", "product-index", builder => builder
                    .WithPagination(0, 50)
                    .Where(f => f
                        .Contains("title", "*hoodie*", caseInsensitive: true)
                        .Or(o => o
                            .Equals("isInStock", true)
                            .Equals("allowPreorder", true)))
                    .SortBy("price", SortOrder.Asc))
                .Build();

            return request;
        }

        /// <summary>
        /// Multiple Where() calls: Filters accumulate with AND logic
        /// </summary>
        public MultiQueryRequest MultipleWhereExample()
        {
            var request = new MultiQueryBuilder()
                .AddQuery("filteredUsers", "user-index", builder => builder
                    .WithPagination(0, 10)
                    .Where(f => f.Equals("status", "active"))
                    .Where(f => f.GreaterThan("age", 18))
                    .Where(f => f.NotEquals("role", "guest"))
                    .SortBy("lastName", SortOrder.Asc))
                .Build();

            return request;
        }

        /// <summary>
        /// Case-insensitive search example
        /// </summary>
        public MultiQueryRequest CaseInsensitiveExample()
        {
            var request = new MultiQueryBuilder()
                .AddQuery("searchResults", "product-index", builder => builder
                    .WithPagination(0, 20)
                    .Where(f => f
                        .Contains("name", "*shirt*", caseInsensitive: true)
                        .Equals("category", "clothing", caseInsensitive: true))
                    .SortBy("relevance", SortOrder.Desc))
                .Build();

            return request;
        }

        /// <summary>
        /// Advanced example: Complex queries with filtering, sorting, and facets
        /// Demonstrates both lambda-based and power-user APIs
        /// </summary>
        public MultiQueryRequest AdvancedExample()
        {
            var request = new MultiQueryBuilder()
                // Query 1: Lambda-based filter API
                .AddQuery("activeUsers", "user-index", builder => builder
                    .WithPagination(0, 25)
                    .Where(f => f.Equals("isActive", true))
                    .SortBy("lastName", SortOrder.Asc)
                    .SortBy("firstName", SortOrder.Asc)
                    .WithFacet("department", size: 10)
                    .WithAliases("summary", "detail"))

                // Query 2: Power-user API with explicit operators
                .AddQuery("recentOrders", "order-index", builder => builder
                    .WithPagination(0, 50)
                    .SortBy("orderDate", SortOrder.Desc)
                    .WhereAll(
                        new GreaterThanOperator<string> { Field = "orderDate", Value = "2025-01-01" },
                        new EqualsOperator<string> { Field = "status", Value = "completed" }))

                // Query 3: Mixed approach
                .AddQuery("topProducts", "product-index", builder => builder
                    .WithPagination(0, 10)
                    .SortBy("salesCount", SortOrder.Desc)
                    .Where(f => f.GreaterThan("stock", 0))
                    .WithFacet("category", "productCategories", 20))

                .Build();

            return request;
        }

        /// <summary>
        /// E-commerce dashboard example: Multiple related queries with lambda filters
        /// </summary>
        public MultiQueryRequest EcommerceDashboardExample()
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            var request = new MultiQueryBuilder()
                // Today's orders
                .AddQuery("todaysOrders", "order-index", builder => builder
                    .WithPagination(0, 100)
                    .Where(f => f.GreaterThan("orderDate", today))
                    .SortBy("orderDate", SortOrder.Desc))

                // Low stock products (need reorder)
                .AddQuery("lowStock", "product-index", builder => builder
                    .WithPagination(0, 50)
                    .Where(f => f.LessThan("stock", 10))
                    .SortBy("stock", SortOrder.Asc))

                // Top customers by order value
                .AddQuery("topCustomers", "customer-index", builder => builder
                    .WithPagination(0, 20)
                    .Where(f => f.GreaterThan("orderCount", 5))
                    .SortBy("totalOrderValue", SortOrder.Desc))

                // Pending shipments with facets
                .AddQuery("pendingShipments", "order-index", builder => builder
                    .WithPagination(0, 100)
                    .Where(f => f.Equals("shipmentStatus", "pending"))
                    .WithFacet("shippingMethod"))

                .Build();

            return request;
        }

        /// <summary>
        /// Content management example: Complex nested filters
        /// </summary>
        public MultiQueryRequest ContentManagementExample()
        {
            var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            var request = new MultiQueryBuilder()
                // Published articles with nested AND
                .AddQuery("publishedArticles", "article-index", builder => builder
                    .WithPagination(0, 15)
                    .Where(f => f
                        .Equals("status", "published")
                        .LessThan("publishDate", now))
                    .SortBy("publishDate", SortOrder.Desc)
                    .WithFacet("category")
                    .WithAliases("listView", "detailView"))

                // Draft content by current user
                .AddQuery("myDrafts", "article-index", builder => builder
                    .WithPagination(0, 10)
                    .Where(f => f
                        .Equals("status", "draft")
                        .Equals("authorId", "current-user-id"))
                    .SortBy("lastModified", SortOrder.Desc))

                // Featured pages
                .AddQuery("featuredPages", "page-index", builder => builder
                    .WithPagination(0, 5)
                    .Where(f => f.Equals("isFeatured", true))
                    .WithAliases("heroView"))

                .Build();

            return request;
        }

        /// <summary>
        /// Mixed approach: Combining fluent builders with pre-constructed queries
        /// </summary>
        public MultiQueryRequest MixedApproachExample()
        {
            // Pre-construct a reusable query
            var standardUserQuery = new QueryObject
            {
                Pagination = new Pagination { Page = 0, PageSize = 10 },
                Sort = new System.Collections.Generic.List<Sort>
                {
                    new Sort { Field = "lastName", Order = SortOrder.Asc }
                }
            };

            var request = new MultiQueryBuilder()
                // Use pre-constructed query
                .AddQuery("allUsers", "user-index", standardUserQuery)

                // Use fluent builder with lambda filter
                .AddQuery("premiumUsers", "user-index", builder => builder
                    .WithPagination(0, 10)
                    .Where(f => f.Equals("subscriptionType", "premium"))
                    .SortBy("lastName", SortOrder.Asc))

                .Build();

            return request;
        }

        /// <summary>
        /// Search results example: Different search criteria with IN operator
        /// </summary>
        public MultiQueryRequest SearchResultsExample(string searchTerm)
        {
            var request = new MultiQueryBuilder()
                // Products matching search term with multiple categories
                .AddQuery("products", "product-index", builder => builder
                    .WithPagination(0, 20)
                    .Where(f => f
                        .Contains("name", searchTerm)
                        .In("category", "clothing", "accessories", "shoes"))
                    .SortBy("relevance", SortOrder.Desc)
                    .WithFacet("category")
                    .WithFacet("brand"))

                // Articles matching search term
                .AddQuery("articles", "article-index", builder => builder
                    .WithPagination(0, 10)
                    .Where(f => f.Contains("title", searchTerm))
                    .SortBy("publishDate", SortOrder.Desc))

                // Users matching search term
                .AddQuery("users", "user-index", builder => builder
                    .WithPagination(0, 10)
                    .Where(f => f.Contains("name", searchTerm))
                    .SortBy("lastName", SortOrder.Asc))

                .Build();

            return request;
        }

        /// <summary>
        /// Complex nested example: Combining AND/OR groups
        /// </summary>
        public MultiQueryRequest ComplexNestedExample()
        {
            var request = new MultiQueryBuilder()
                .AddQuery("complexQuery", "product-index", builder => builder
                    .WithPagination(0, 50)
                    .Where(f => f
                        // Main condition: active products
                        .Equals("status", "active")
                        // AND: Must be in stock OR allow preorder
                        .Or(o => o
                            .Equals("isInStock", true)
                            .Equals("allowPreorder", true))
                        // AND: Price range
                        .And(a => a
                            .GreaterThanOrEquals("price", 10)
                            .LessThanOrEquals("price", 100)))
                    .SortBy("relevance", SortOrder.Desc)
                    .WithFacet("brand")
                    .WithFacet("category"))
                .Build();

            return request;
        }

        /// <summary>
        /// Demonstrates validation and error handling
        /// </summary>
        public void ValidationExamples()
        {
            var builder = new MultiQueryBuilder();

            // Check if key already exists before adding
            if (!builder.ContainsKey("users"))
            {
                builder.AddQuery("users", "user-index", q => q.WithPagination(0, 10));
            }

            // Check query count
            Console.WriteLine($"Current query count: {builder.Count}");

            // Build will throw if no queries added
            try
            {
                new MultiQueryBuilder().Build();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Duplicate keys will throw
            try
            {
                new MultiQueryBuilder()
                    .AddQuery("users", "index1", q => { })
                    .AddQuery("users", "index2", q => { }); // This will throw
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // Maximum 5 queries per request
            try
            {
                new MultiQueryBuilder()
                    .AddQuery("q1", "index", q => { })
                    .AddQuery("q2", "index", q => { })
                    .AddQuery("q3", "index", q => { })
                    .AddQuery("q4", "index", q => { })
                    .AddQuery("q5", "index", q => { })
                    .AddQuery("q6", "index", q => { }); // This will throw
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
