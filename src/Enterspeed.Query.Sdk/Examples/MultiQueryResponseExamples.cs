using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Examples
{
    /// <summary>
    /// Example usage of the fluent multi-query response API.
    /// Demonstrates how to build queries, execute them, and handle typed responses.
    /// API response format: https://docs.enterspeed.com/api#tag/Query/operation/queryMultiContentPost
    /// </summary>
    public class MultiQueryResponseExamples
    {
        // Example model classes matching API documentation
        public class Product
        {
            public string Sku { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
        }

        public class User
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
        }

        /// <summary>
        /// Complete integration example showing the full workflow
        /// API Response Format: [{ index, name, totalResults, results, facets }, ...]
        /// </summary>
        public async Task CompleteIntegrationExample(IEnterspeedQueryService queryService, string apiKey)
        {
            // 1. Build a multi-query request
            var request = new MultiQueryBuilder()
                .AddQuery("products", "products-index", builder => builder
                    .Where(f => f.GreaterThan("stock", "0"))
                    .SortBy("name", SortOrder.Asc)
                    .WithPagination(0, 20))
                .AddQuery("activeUsers", "user-index", builder => builder
                    .Where(f => f
                        .Equals("status", "active")
                        .GreaterThan("age", "18"))
                    .WithPagination(0, 10))
                .Build();

            // 2. Execute the multi-query
            // The API returns: [{ index: "products-index", name: "products", totalResults: 200, results: [...], facets: [...] }, ...]
            MultiQueryApiResponse response = await queryService.Query(apiKey, request, CancellationToken.None);

            // 3. Check HTTP-level success
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Request failed: {response.Message}");
                return;
            }

            // 4. Retrieve strongly-typed responses by query name
            var productsResponse = response.Get<Product>("products");
            var usersResponse = response.Get<User>("activeUsers");

            // 5. Handle products query
            if (productsResponse.Status)
            {
                var productsSuccess = productsResponse as ISuccess<Product>;

                Console.WriteLine($"Found {productsSuccess.TotalResults} products");
                foreach (var product in productsSuccess.Results)
                {
                    Console.WriteLine($"  - {product.Name} ({product.Sku})");
                }

                // Access facets
                foreach (var facet in productsSuccess.Facets)
                {
                    Console.WriteLine($"\n{facet.Name}:");
                    foreach (var group in facet.Groups)
                    {
                        Console.WriteLine($"  {group.Value}: {group.Count}");
                    }
                }
            }
            else
            {
                var failure = productsResponse as ErrorResponse<Product>;
                Console.WriteLine("Products query failed:");
                foreach (var error in failure.Errors)
                {
                    Console.WriteLine($"  {error.Message}");
                }
            }

            // 6. Handle users query independently
            if (usersResponse.Status)
            {
                var usersSuccess = usersResponse as ISuccess<User>;
                Console.WriteLine($"\nFound {usersSuccess.TotalResults} active users");

                foreach (var user in usersSuccess.Results)
                {
                    Console.WriteLine($"  - {user.Name} ({user.Email})");
                }
            }
        }

        /// <summary>
        /// Pattern matching example for clean response handling
        /// </summary>
        public void PatternMatchingExample(MultiQueryApiResponse response)
        {
            var productsResponse = response.Get<Product>("products");

            switch (productsResponse)
            {
                case ISuccess<Product> success:
                    Console.WriteLine($"Success! Got {success.TotalResults} results");
                    foreach (var product in success.Results)
                    {
                        Console.WriteLine($"  - {product.Name}");
                    }
                    break;

                case ErrorResponse<Product> failure:
                    Console.WriteLine("Query failed:");
                    foreach (var error in failure.Errors)
                    {
                        Console.WriteLine($"  - {error.Message}");
                    }
                    break;
            }
        }

        /// <summary>
        /// Partial failures example - one query can fail without affecting others
        /// </summary>
        public void PartialFailuresExample(MultiQueryApiResponse response)
        {
            var productsResponse = response.Get<Product>("products");
            var usersResponse = response.Get<User>("users");

            // Both queries execute independently
            Console.WriteLine($"Products: {(productsResponse.Status ? "✓" : "✗")}");
            Console.WriteLine($"Users: {(usersResponse.Status ? "✓" : "✗")}");

            // Process successful queries
            if (productsResponse.Status)
            {
                var products = (productsResponse as ISuccess<Product>).Results;
                // Process products...
            }

            if (usersResponse.Status)
            {
                var users = (usersResponse as ISuccess<User>).Results;
                // Process users...
            }
        }

        /// <summary>
        /// Type mismatch handling - graceful failure instead of exceptions
        /// </summary>
        public void TypeMismatchExample(MultiQueryApiResponse response)
        {
            // Assume "products" query returns Product objects
            var wrongType = response.Get<User>("products"); // Wrong type!

            if (!wrongType.Status)
            {
                var failure = wrongType as ErrorResponse<User>;
                // Error message will indicate type mismatch
                Console.WriteLine($"{failure.Errors[0].Message}");
            }
        }

        /// <summary>
        /// Missing query key handling
        /// </summary>
        public void MissingKeyExample(MultiQueryApiResponse response)
        {
            // Option 1: Check before retrieving
            if (response.ContainsQuery("products"))
            {
                var products = response.Get<Product>("products");
                // Process...
            }

            // Option 2: Handle failure response
            var nonExistent = response.Get<Product>("nonExistent");
            if (!nonExistent.Status)
            {
                var failure = nonExistent as ErrorResponse<Product>;
                // Error code will be QUERY_NOT_FOUND
                Console.WriteLine($"Query not found: {failure.Errors[0].Message}");
            }
        }

        /// <summary>
        /// Inspecting available queries
        /// </summary>
        public void InspectQueriesExample(MultiQueryApiResponse response)
        {
            var queryNames = response.GetQueryNames();
            Console.WriteLine($"Response contains {queryNames.Count} queries:");
            foreach (var name in queryNames)
            {
                Console.WriteLine($"  - {name}");
            }
        }

        /// <summary>
        /// Working with facets for aggregations
        /// </summary>
        public void FacetsExample(MultiQueryApiResponse response)
        {
            var productsResponse = response.Get<Product>("products");

            if (productsResponse is ISuccess<Product> success)
            {
                Console.WriteLine($"Total results: {success.TotalResults}");

                foreach (var facet in success.Facets)
                {
                    Console.WriteLine($"\n{facet.Name} ({facet.Field}):");
                    foreach (var group in facet.Groups)
                    {
                        Console.WriteLine($"  {group.Value}: {group.Count}");
                    }
                }
            }
        }
    }
}
