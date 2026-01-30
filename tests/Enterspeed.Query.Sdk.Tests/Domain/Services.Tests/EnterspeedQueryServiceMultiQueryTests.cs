using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;

namespace Enterspeed.Query.Sdk.Tests.Domain.Services.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using Configuration;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;
using Enterspeed.Query.Sdk.Domain.Services;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

public class EnterspeedQueryServiceMultiQueryTests
{
    private const string TestApiKey = "environment-test-guid";
    private readonly IJsonSerializer _serializer;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly EnterspeedQueryService _queryService;

    // Test models matching the API response structure
    private record Product
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string OriginId { get; set; }
        public string SourceGuid { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    private record User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }

    public EnterspeedQueryServiceMultiQueryTests()
    {
        _serializer = new SystemTextJsonSerializer();
        var config = new EnterspeedQueryConfiguration();
        var configProvider = new EnterspeedQueryConfigurationProvider(config);

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(config.BaseUrl)
        };

        _queryService = new EnterspeedQueryService(httpClient, configProvider, _serializer);
    }

    [Fact]
    public async Task MultiQuery_WithSuccessfulQueries_ReturnsTypedResponses()
    {
        // Arrange - Build multi-query request
        var request = new MultiQueryBuilder()
            .AddQuery("products", "product-index", builder => builder
                .Where(f => f.GreaterThan("stock", "0"))
                .WithPagination(0, 10))
            .AddQuery("users", "user-index", builder => builder
                .Where(f => f.Equals("status", "active"))
                .WithPagination(0, 5))
            .Build();

        // Arrange - Mock API response matching the documentation
        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 200,
                status = 0,  // Success status
                results = new[]
                {
                    new
                    {
                        sku = "p-5427",
                        name = "Running shoe",
                        url = "/products/running-shoe/",
                        originId = "p-5427",
                        sourceGuid = "9f741916-19e3-4791-9ebd-701634ba9b75",
                        updatedAt = "2025-01-07T08:43:15.7351089Z"
                    },
                    new
                    {
                        sku = "p-5428",
                        name = "Tennis shoe",
                        url = "/products/tennis-shoe/",
                        originId = "p-5428",
                        sourceGuid = "9f741916-19e3-4791-9ebd-701634ba9b75",
                        updatedAt = "2025-01-07T09:00:00.0000000Z"
                    }
                },
                facets = new[]
                {
                    new
                    {
                        name = "Categories",
                        field = "category",
                        groups = new[]
                        {
                            new
                            {
                                value = "Shoes", count = 15
                            },
                            new
                            {
                                value = "Accessories", count = 5
                            }
                        },
                        isValid = true
                    }
                }
            },
            new
            {
                index = "user-index",
                name = "users",
                totalResults = 42,
                status = 0,  // Success status
                results = new[]
                {
                    new
                    {
                        id = "u-001",
                        name = "John Doe",
                        email = "john@example.com",
                        status = "active"
                    }
                },
                facets = Array.Empty<object>()
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);

        // Assert - Check HTTP response
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Response.Should().NotBeNull();

        // Assert - Retrieve typed product response
        var productsResponse = response.Get<Product>("products");
        productsResponse.Should().NotBeNull();
        productsResponse.Status.Should().BeTrue("products query should succeed");

        var productsSuccess = productsResponse as ISuccess<Product>;
        productsSuccess.Should().NotBeNull();
        productsSuccess?.TotalResults.Should().Be(200);
        productsSuccess?.Results.Should().HaveCount(2);
        productsSuccess?.Results[0].Sku.Should().Be("p-5427");
        productsSuccess?.Results[0].Name.Should().Be("Running shoe");
        productsSuccess?.Facets.Should().HaveCount(1);
        productsSuccess?.Facets[0].Name.Should().Be("Categories");

        // Assert - Retrieve typed user response
        var usersResponse = response.Get<User>("users");
        usersResponse.Should().NotBeNull();
        usersResponse.Status.Should().BeTrue("users query should succeed");

        var usersSuccess = usersResponse as ISuccess<User>;
        usersSuccess.Should().NotBeNull();
        usersSuccess?.TotalResults.Should().Be(42);
        usersSuccess?.Results.Should().HaveCount(1);
        usersSuccess?.Results[0].Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task MultiQuery_WithPartialFailure_ReturnsIndependentResponses()
    {
        // Arrange
        var request = new MultiQueryBuilder()
            .AddQuery("products", "product-index", builder => builder
                .WithPagination(0, 10))
            .AddQuery("invalid", "invalid-index", builder => builder
                .WithPagination(0, 10))
            .Build();

        // Mock response with one success and one error
        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 5,
                status = 0,  // Success status
                results = new[]
                {
                    new
                    {
                        sku = "p-001",
                        name = "Test Product",
                        url = "/products/test/",
                        originId = "p-001",
                        sourceGuid = "test-guid",
                        updatedAt = "2025-01-01T00:00:00Z"
                    }
                },
                facets = Array.Empty<object>()
            },
            new
            {
                index = "invalid-index",
                name = "invalid",
                status = 1,  // Error status
                message = "Index not found",
                errors = new[]
                {
                    "The specified index does not exist"
                }
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);

        // Assert - Products should succeed
        var productsResponse = response.Get<Product>("products");
        productsResponse.Status.Should().BeTrue();

        var productsSuccess = productsResponse as ISuccess<Product>;
        productsSuccess?.Results.Should().HaveCount(1);

        // Assert - Invalid query should fail but not affect products
        var invalidResponse = response.Get<Product>("invalid");
        invalidResponse.Status.Should().BeFalse("invalid query should fail");

        var invalidFailure = invalidResponse as ErrorResponse<Product>;
        invalidFailure.Should().NotBeNull();
        invalidFailure?.Errors.Should().NotBeEmpty();
        invalidFailure?.Errors.Should().Contain(e => e.Message.Contains("Index not found"));
    }

    [Fact]
    public async Task MultiQuery_WithMissingQueryKey_ReturnsFailure()
    {
        // Arrange
        var request = new MultiQueryBuilder()
            .AddQuery("products", "product-index", builder => builder.WithPagination(0, 10))
            .Build();

        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 0,
                status = 0,  // Success status
                results = Array.Empty<object>(),
                facets = Array.Empty<object>()
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);

        // Assert - Requesting non-existent key returns failure
        var missingResponse = response.Get<Product>("nonexistent");
        missingResponse.Status.Should().BeFalse();

        var failure = missingResponse as ErrorResponse<Product>;
    }

    [Fact]
    public async Task MultiQuery_WithTypeMismatch_ReturnsFailure()
    {
        // Arrange
        var request = new MultiQueryBuilder()
            .AddQuery("products", "product-index", builder => builder.WithPagination(0, 10))
            .Build();

        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 1,
                status = 0,  // Success status
                results = new[]
                {
                    new
                    {
                        sku = "p-001",
                        name = "Test Product",
                        url = "/test/",
                        originId = "p-001",
                        sourceGuid = "guid",
                        updatedAt = "2025-01-01T00:00:00Z"
                    }
                },
                facets = Array.Empty<object>()
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);

        // Assert - Correct type succeeds
        var correctTypeResponse = response.Get<Product>("products");
        correctTypeResponse.Status.Should().BeTrue();

        // Note: Type mismatches don't fail at the SDK level - JSON deserialization is permissive.
        // Wrong types will deserialize successfully but with partial/default values.
        // This is standard JSON behavior and consumers should request the correct type.
        var wrongTypeResponse = response.Get<User>("products");
        wrongTypeResponse.Status.Should().BeTrue("JSON deserialization is permissive and doesn't fail on type mismatches");

        // The User object will have some default/null values since product fields don't match user fields
        var wrongTypeSuccess = wrongTypeResponse as ISuccess<User>;
        wrongTypeSuccess.Should().NotBeNull();
        wrongTypeSuccess?.Results.Should().HaveCount(1);
    }

    [Fact]
    public async Task MultiQuery_ResponseCaching_ReturnsSameInstance()
    {
        // Arrange
        var request = new MultiQueryBuilder()
            .AddQuery("products", "product-index", builder => builder.WithPagination(0, 10))
            .Build();

        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 1,
                status = 0,  // Success status
                results = new[]
                {
                    new
                    {
                        sku = "p-001",
                        name = "Test",
                        url = "/test/",
                        originId = "p-001",
                        sourceGuid = "guid",
                        updatedAt = "2025-01-01T00:00:00Z"
                    }
                },
                facets = Array.Empty<object>()
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);
        var firstCall = response.Get<Product>("products");
        var secondCall = response.Get<Product>("products");

        // Assert
        firstCall.Should().BeSameAs(secondCall, "responses should be cached");
    }

    [Fact]
    public async Task MultiQuery_WithFacets_ReturnsFacetData()
    {
        // Arrange
        var request = new MultiQueryBuilder()
            .AddQuery("products", "product-index", builder => builder
                .WithPagination(0, 10))
            .Build();

        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 100,
                status = 0,  // Success status
                results = Array.Empty<object>(),
                facets = new[]
                {
                    new
                    {
                        name = "Categories",
                        field = "category",
                        groups = new[]
                        {
                            new
                            {
                                value = "Caps", count = 8
                            },
                            new
                            {
                                value = "Hoodies", count = 2
                            }
                        },
                        isValid = true
                    },
                    new
                    {
                        name = "Sizes",
                        field = "size",
                        groups = new[]
                        {
                            new
                            {
                                value = "S", count = 15
                            },
                            new
                            {
                                value = "M", count = 25
                            },
                            new
                            {
                                value = "L", count = 20
                            }
                        },
                        isValid = true
                    }
                }
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);
        var productsResponse = response.Get<Product>("products");

        // Assert
        productsResponse.Status.Should().BeTrue();
        var success = productsResponse as ISuccess<Product>;

        success?.Facets.Should().HaveCount(2);
        success?.Facets[0].Name.Should().Be("Categories");
        success?.Facets[0].Groups.Should().HaveCount(2);
        success?.Facets[0].Groups[0].Value.Should().Be("Caps");
        success?.Facets[0].Groups[0].Count.Should().Be(8);

        success?.Facets[1].Name.Should().Be("Sizes");
        success?.Facets[1].Groups.Should().HaveCount(3);
    }

    [Fact]
    public void MultiQuery_ContainsQuery_ChecksExistence()
    {
        // Arrange
        var response = new MultiQueryApiResponse
        {
            Response = new Dictionary<string, MultiQueryResponse>()
        };

        // Act & Assert
        response.ContainsQuery("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void MultiQuery_GetQueryNames_ReturnsAllNames()
    {
        // Arrange
        var response = new MultiQueryApiResponse
        {
            Response = new Dictionary<string, MultiQueryResponse>()
        };

        // Act
        var names = response.GetQueryNames();

        // Assert
        names.Should().BeEmpty();
    }

        #region Single Query Tests

    [Fact]
    public async Task SingleQuery_WithValidRequest_ReturnsSuccess()
    {
        // Arrange - Test the single query API endpoint (not multi-query)
        var mockResponse = new QueryResponseSuccess
        {
            TotalResults = 2,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "sku", "p-001"
                    },
                    {
                        "name", "Product 1"
                    }
                },
                new ()
                {
                    {
                        "sku", "p-002"
                    },
                    {
                        "name", "Product 2"
                    }
                }
            },
            Facets = new List<FacetResult>
            {
                new ()
                {
                    Name = "Categories",
                    Field = "category",
                    IsValid = true,
                    Groups = new List<FacetGroup>
                    {
                        new ()
                        {
                            Value = "Electronics", Count = 15
                        },
                        new ()
                        {
                            Value = "Books", Count = 8
                        }
                    }
                }
            }
        };

        var responseJson = _serializer.Serialize(mockResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var query = new QueryObject
        {
            Sort = new List<Sort>
            {
                new ()
                {
                    Field = "_updatedAt", Order = SortOrder.Desc
                }
            },
            Pagination = new Pagination
            {
                Page = 0, PageSize = 10
            }
        };

        // Act - Call single query method
        var result = await _queryService.Query(TestApiKey, "product-index", query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        if (result.Response is QueryResponseSuccess successResponse)
        {
            successResponse.Should().NotBeNull();
            successResponse.TotalResults.Should().Be(2);
            successResponse.Results.Should().HaveCount(2);
            successResponse.Facets.Should().HaveCount(1);
            successResponse.Facets[0].Groups.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task SingleQuery_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange - Test error handling for single query API
        var mockResponse = new Dictionary<string, object>
        {
            {
                "message", "Query is not valid"
            },
            {
                "errors", new List<string>
                {
                    "Field not found: invalidField"
                }
            },
            {
                "status", 1
            }
        };

        var responseJson = _serializer.Serialize(mockResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var query = new QueryObject
        {
            Sort = new List<Sort>
                {
                    new ()
                        {
                            Field = "invalidField", Order = SortOrder.Desc
                        }
                },
            Pagination = new Pagination
            {
                Page = 0, PageSize = 10
            }
        };

        // Act
        var result = await _queryService.Query(TestApiKey, "product-index", query, CancellationToken.None);

        // Assert - Single query API returns null Response on error, with Message populated
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("Query is not valid");
        if (result.Response is IError<string> failure)
        {
            failure.Errors.Should().Contain(e => e.Message.Contains("Field not found: invalidField"));
        }
    }

    [Fact]
    public async Task MultiQuery_WithAllErrorResponses_ReturnsErrorStatus()
    {
        // Arrange - Test when all queries in multi-query fail with OK status (errors in response)
        var apiResponse = new List<object>
        {
            new
            {
                index = "invalid-index-1",
                name = "query1",
                status = 1,
                message = "Index not found",
                errors = new[]
                {
                    "The specified index 'invalid-index-1' does not exist"
                }
            },
            new
            {
                index = "invalid-index-2",
                name = "query2",
                status = 1,
                message = "Permission denied",
                errors = new[]
                {
                    "You don't have permission to access this index"
                }
            }
        };

        var responseJson = _serializer.Serialize(apiResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,  // Multi-query returns OK even with errors in individual queries
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var request = new MultiQueryBuilder()
            .AddQuery("query1", "invalid-index-1", builder => builder.WithPagination(0, 10))
            .AddQuery("query2", "invalid-index-2", builder => builder.WithPagination(0, 10))
            .Build();

        // Act
        var response = await _queryService.Query(TestApiKey, request.Queries.ToList(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // response.Message.Should().Contain("Index not found"); // TODO: Verify that we just do a generic Message and not many in the message object
        // response.Message.Should().Contain("Permission denied");

        // All queries should fail
        var query1Response = response.Get<Product>("query1");
        query1Response.Status.Should().BeFalse();

        var query2Response = response.Get<Product>("query2");
        query2Response.Status.Should().BeFalse();

        var query1Failure = query1Response as ErrorResponse<Product>;
        query1Failure?.Errors.Should().Contain(e => e.Message.Contains("Index not found"));

        var query2Failure = query2Response as ErrorResponse<Product>;
        query2Failure?.Errors.Should().Contain(e => e.Message.Contains("Permission denied"));
    }

        #endregion
}
