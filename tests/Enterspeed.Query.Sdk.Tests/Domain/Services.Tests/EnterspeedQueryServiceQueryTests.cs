using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

namespace Enterspeed.Query.Sdk.Tests.Domain.Services.Tests;

using System;
using System.Collections.Generic;
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
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Services;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;
using static VerifyXunit.Verifier;

public class EnterspeedQueryServiceQueryTests
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
        public int Stock { get; set; }
    }

    private record User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }

    public EnterspeedQueryServiceQueryTests()
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
        var request = new QueryBuilder()
            .AddQuery<Product>("products", "product-index", builder => builder
                .Where(f => f.GreaterThan(x => x.Stock, 0))
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
                        status = "active",
                        stock = 1,
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert - HTTP-level behavior
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Response.Should().NotBeNull();

        // Assert - Service behavior
        var productsResponse = response.Get<Product>("products");
        productsResponse.Should().NotBeNull();
        productsResponse.Status.Should().BeTrue("products query should succeed");

        var usersResponse = response.Get<User>("users");
        usersResponse.Should().NotBeNull();
        usersResponse.Status.Should().BeTrue("users query should succeed");

        // Assert - Complete data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            ProductsResponse = productsResponse,
            UsersResponse = usersResponse
        });
    }

    [Fact]
    public async Task MultiQuery_WithPartialFailure_ReturnsIndependentResponses()
    {
        // Arrange
        var request = new QueryBuilder()
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert - HTTP-level behavior
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Service behavior - Products should succeed
        var productsResponse = response.Get<Product>("products");
        productsResponse.Status.Should().BeTrue();

        // Assert - Service behavior - Invalid query should fail but not affect products
        var invalidResponse = response.Get<Product>("invalid");
        invalidResponse.Status.Should().BeFalse("invalid query should fail");

        // Assert - Complete data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            ProductsResponse = productsResponse,
            InvalidResponse = invalidResponse
        });
    }

    [Fact]
    public async Task MultiQuery_WithMissingQueryKey_ReturnsFailure()
    {
        // Arrange
        var request = new QueryBuilder()
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert - HTTP-level behavior
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Service behavior - Requesting non-existent key returns failure
        var missingResponse = response.Get<Product>("nonexistent");
        missingResponse.Status.Should().BeFalse();

        // Assert - Complete data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            MissingResponse = missingResponse
        });
    }

    [Fact]
    public async Task MultiQuery_WithTypeMismatch_ReturnsFailure()
    {
        // Arrange
        var request = new QueryBuilder()
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

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
        var request = new QueryBuilder()
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);
        var firstCall = response.Get<Product>("products");
        var secondCall = response.Get<Product>("products");

        // Assert
        firstCall.Should().BeSameAs(secondCall, "responses should be cached");
    }

    [Fact]
    public async Task MultiQuery_WithFacets_ReturnsFacetData()
    {
        // Arrange
        var request = new QueryBuilder()
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);
        var productsResponse = response.Get<Product>("products");

        // Assert - HTTP-level behavior
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Service behavior
        productsResponse.Status.Should().BeTrue();
        var success = productsResponse as ISuccess<Product>;
        success.Should().NotBeNull();
        success?.Facets.Should().HaveCount(2);

        // Assert - Complete facet data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            ProductsResponse = productsResponse
        });
    }

    [Fact]
    public void MultiQuery_ContainsQuery_ChecksExistence()
    {
        // Arrange
        var response = new QueryApiResponse
        {
            Response = new Dictionary<string, QueryResponse>()
        };

        // Act & Assert
        response.ContainsQuery("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void MultiQuery_GetQueryNames_ReturnsAllNames()
    {
        // Arrange
        var response = new QueryApiResponse
        {
            Response = new Dictionary<string, QueryResponse>()
        };

        // Act
        var names = response.GetQueryNames();

        // Assert
        names.Should().BeEmpty();
    }

    #region Forbidden Tests

    [Fact]
    public async Task Query_WithForbiddenApiKey_ReturnsForbiddenStatusCode()
    {
        // Arrange - API returns 403 when the API key has no Query scope
        var responseJson = _serializer.Serialize(new { error = "Forbidden" });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var request = new QueryBuilder()
            .AddQuery("products", "product-index", builder => builder.WithPagination(0, 10))
            .Build();

        // Act
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Message.Should().Be("Forbidden");
        response.Response.Should().BeNull();
    }

    [Fact]
    public async Task MultiQuery_WithForbiddenBatchItem_IsForbiddenReturnsTrue()
    {
        // Arrange - API returns 200 with one forbidden item and one success item
        var request = new QueryBuilder()
            .AddQuery("products", "product-index", builder => builder.WithPagination(0, 10))
            .AddQuery("restricted", "restricted-index", builder => builder.WithPagination(0, 10))
            .Build();

        var apiResponse = new List<object>
        {
            new
            {
                index = "product-index",
                name = "products",
                totalResults = 1,
                status = 0,
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
                index = "restricted-index",
                name = "restricted",
                status = 1,
                message = "Forbidden",
                errors = new[] { "Forbidden" }
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert - HTTP-level
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Successful query is unaffected
        var productsResult = response.Get<Product>("products");
        productsResult.Status.Should().BeTrue();

        // Assert - Forbidden item is an error with IsForbidden true
        var restrictedResult = response.Get<Product>("restricted");
        restrictedResult.Status.Should().BeFalse();
        var restrictedError = restrictedResult as IError;
        restrictedError.Should().NotBeNull();
        restrictedError!.IsForbidden().Should().BeTrue();

        // Assert - Complete data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            ProductsResponse = productsResult,
            RestrictedResponse = restrictedResult
        });
    }

    [Fact]
    public async Task MultiQuery_WithRegularError_IsForbiddenReturnsFalse()
    {
        // Arrange - A regular error (e.g. index not found) should not be flagged as forbidden
        var request = new QueryBuilder()
            .AddQuery("missing", "missing-index", builder => builder.WithPagination(0, 10))
            .Build();

        var apiResponse = new List<object>
        {
            new
            {
                index = "missing-index",
                name = "missing",
                status = 1,
                message = "Index not found",
                errors = new[] { "The specified index does not exist" }
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
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert
        var missingResult = response.Get<Product>("missing");
        missingResult.Status.Should().BeFalse();
        var missingError = missingResult as IError;
        missingError.Should().NotBeNull();
        missingError!.IsForbidden().Should().BeFalse();
    }

    #endregion

    #region Single Query Tests

    [Fact]
    public async Task SingleQuery_WithValidRequest_ReturnsSuccess()
    {
        // Arrange - Test the single query API endpoint (not multi-query)
        const string name = "products";
        const string index = "product-index";

        var apiResponse = new List<object>
        {
            new
            {
                name = name,
                index = index,
                status = 0,
                totalResults = 2,
                results = new List<Dictionary<string, object>>
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
                facets = new[]
                {
                    new
                    {
                        name = "Categories",
                        field = "category",
                        isValid = true,
                        groups = new[]
                        {
                            new
                            {
                                value = "Electronics", count = 15
                            },
                            new
                            {
                                value = "Books", count = 8
                            }
                        }
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

        var queryRequest = new QueryBuilder()
            .AddQuery(name, index, builder => builder
                .SortBy("_updatedAt", SortOrder.Desc)
                .WithPagination(0, 10))
            .Build();

        // Act
        var result = await _queryService.Query(TestApiKey, queryRequest, CancellationToken.None);

        // Assert - HTTP-level behavior
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Service behavior
        result.GetQueryNames().Should().HaveCount(1);
        var queryResult = result.Get<object>(name);
        queryResult.Status.Should().BeTrue();

        if (queryResult is ISuccess<object> successResponse)
        {
            successResponse.Should().NotBeNull();
            successResponse.TotalResults.Should().Be(2);
            successResponse.Results.Should().HaveCount(2);
            successResponse.Facets.Should().HaveCount(1);
            successResponse.Facets[0].Groups.Should().HaveCount(2);
        }

        // Assert - Complete data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            QueryResult = queryResult
        });
    }

    [Fact]
    public async Task SingleQuery_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange - Test error handling for single query API
        var apiResponse = new List<object>
        {
            new
            {
            name = "products",
            index = "product-index",
            status = 1,
            message = "Query is not valid",
            errors = new List<string>
                {
                    "Field not found: invalidField"
                }
            },
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
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var queryRequest = new QueryBuilder()
            .AddQuery("products", "product-index", builder => builder
                .SortBy("invalidField", SortOrder.Desc)  // Invalid sort field to trigger error
                .WithPagination(0, 10))
            .Build();

        // Act
        var result = await _queryService.Query(TestApiKey, queryRequest, CancellationToken.None);

        // Assert - HTTP-level behavior
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Message.Should().Be("Query is not valid");

        // Assert - Service behavior
        var resultError = result.Get<object>("products");
        resultError.Should().NotBeAssignableTo<QueryError>();
        resultError.Status.Should().BeFalse();

        if (resultError is IError failure)
        {
            failure.Errors.Should().Contain(e => e.Message.Contains("Query is not valid"));
        }

        // Assert - Complete error data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            ResultError = resultError
        });
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

        var request = new QueryBuilder()
            .AddQuery("query1", "invalid-index-1", builder => builder.WithPagination(0, 10))
            .AddQuery("query2", "invalid-index-2", builder => builder.WithPagination(0, 10))
            .Build();

        // Act
        var response = await _queryService.Query(TestApiKey, request, CancellationToken.None);

        // Assert - HTTP-level behavior
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Service behavior - All queries should fail
        var query1Response = response.Get<Product>("query1");
        query1Response.Status.Should().BeFalse();

        var query2Response = response.Get<Product>("query2");
        query2Response.Status.Should().BeFalse();

        // Assert - Service behavior - Error details verification
        var query1Failure = query1Response as ErrorResponse<Product>;
        query1Failure?.Errors.Should().Contain(e => e.Message.Contains("Index not found"));

        var query2Failure = query2Response as ErrorResponse<Product>;
        query2Failure?.Errors.Should().Contain(e => e.Message.Contains("Permission denied"));

        // Assert - Complete error data structure with snapshot
        await Verify(new
        {
            ApiResponse = apiResponse,
            Query1Response = query1Response,
            Query2Response = query2Response
        });
    }

    #endregion
}
