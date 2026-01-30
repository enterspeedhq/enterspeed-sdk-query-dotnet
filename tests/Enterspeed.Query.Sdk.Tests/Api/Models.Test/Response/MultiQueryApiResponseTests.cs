using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;

namespace Enterspeed.Query.Sdk.Tests.Api.Models.Test.Response;
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
using Configuration;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;
using Enterspeed.Query.Sdk.Domain.Services;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using FluentAssertions;
using Moq;
using Moq.Protected;
using static VerifyXunit.Verifier;

using Xunit;

public class MultiQueryApiResponseTests
{
    private record TestBook
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
    }

    private record TestAuthor
    {
        public string Name { get; set; }
        public string Country { get; set; }
    }

    [Fact]
    public Task Get_WithNullQueryName_ReturnsFailure()
    {
        // Arrange
        var response = new MultiQueryApiResponse
        {
            Response = new Dictionary<string, MultiQueryResponse>()
        };

        // Act
        var result = response.Get<TestBook>(null);

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task Get_WithEmptyQueryName_ReturnsFailure()
    {
        // Arrange
        var response = new MultiQueryApiResponse
        {
            Response = new Dictionary<string, MultiQueryResponse>()
        };

        // Act
        var result = response.Get<TestBook>(string.Empty);

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task Get_WithNullResponseList_ReturnsFailure()
    {
        // Arrange
        var response = new MultiQueryApiResponse
        {
            Response = null
        };

        // Act
        var result = response.Get<TestBook>("books");

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task Get_WithMissingQueryKey_ReturnsFailure()
    {
        // Arrange
        var responseList = new List<MultiQueryResponse>();
        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var result = response.Get<TestBook>("nonexistent");

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task Get_WithErrorResponse_ReturnsFailure()
    {
        // Arrange
        var errorResponse = new MultiQueryResponseError
        {
            Name = "books",
            Index = "books-index",
            Message = "Query execution failed",
            Errors = new[]
            {
                "Invalid filter", "Timeout occurred"
            }
        };

        var responseList = new List<MultiQueryResponse>
        {
            errorResponse
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var result = response.Get<TestBook>("books");

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task Get_WithSuccessResponse_ReturnsSuccess()
    {
        // Arrange
        var successResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index",
            TotalResults = 2,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "title", "Book 1"
                    },
                    {
                        "author", "Author 1"
                    },
                    {
                        "year", 2021
                    }
                },
                new ()
                {
                    {
                        "title", "Book 2"
                    },
                    {
                        "author", "Author 2"
                    },
                    {
                        "year", 2022
                    }
                }
            }
        };

        var responseList = new List<MultiQueryResponse>
        {
            successResponse
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var result = response.Get<TestBook>("books");

        // Assert
        return Verify(result);
    }

    [Fact]
    public Task Get_WithSuccessResponse_ValueReturnsFirstResult()
    {
        // Arrange
        var successResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index",
            TotalResults = 2,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "title", "First Book"
                    },
                    {
                        "author", "First Author"
                    },
                    {
                        "year", 2021
                    }
                },
                new ()
                {
                    {
                        "title", "Second Book"
                    },
                    {
                        "author", "Second Author"
                    },
                    {
                        "year", 2022
                    }
                }
            }
        };

        var responseList = new List<MultiQueryResponse>
        {
            successResponse
        };
        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var result = response.Get<TestBook>("books");
        var success = result as ISuccess<TestBook>;
        var firstBook = success?.Value();

        // Assert
        return Verify(firstBook);
    }

    [Fact]
    public Task Get_WithMultipleQueries_ReturnsIndependentResponses()
    {
        // Arrange
        var booksResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index",
            TotalResults = 1,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "title", "Test Book"
                    },
                    {
                        "author", "Test Author"
                    },
                    {
                        "year", 2021
                    }
                }
            }
        };

        var authorsResponse = new MultiQueryResponseSuccess
        {
            Name = "authors",
            Index = "authors-index",
            TotalResults = 1,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "name", "Test Author"
                    },
                    {
                        "country", "Denmark"
                    }
                }
            }
        };

        var responseList = new List<MultiQueryResponse>
        {
            booksResponse,
            authorsResponse
        };
        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var booksResult = response.Get<TestBook>("books");
        var authorsResult = response.Get<TestAuthor>("authors");

        // Assert
        return Verify(new
        {
            booksResult, authorsResult
        });
    }

    [Fact]
    public Task Get_WithPartialFailure_IndependentQueriesNotAffected()
    {
        // Arrange
        var successResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index",
            TotalResults = 1,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "title", "Test Book"
                    },
                    {
                        "author", "Test Author"
                    },
                    {
                        "year", 2021
                    }
                }
            }
        };

        var errorResponse = new MultiQueryResponseError
        {
            Name = "authors",
            Index = "authors-index",
            Message = "Query failed"
        };

        var responseList = new List<MultiQueryResponse>
        {
            successResponse,
            errorResponse
        };
        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var booksResult = response.Get<TestBook>("books");
        var authorsResult = response.Get<TestAuthor>("authors");

        // Assert
        return Verify(new
        {
            booksResult, authorsResult
        });
    }

    [Fact]
    public void Get_CachesResultsPerQueryAndType()
    {
        // Arrange
        var successResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index",
            TotalResults = 1,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "title", "Test Book"
                    },
                    {
                        "author", "Test Author"
                    },
                    {
                        "year", 2021
                    }
                }
            }
        };

        var responseList = new List<MultiQueryResponse>
        {
            successResponse
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var result1 = response.Get<TestBook>("books");
        var result2 = response.Get<TestBook>("books");

        // Assert
        result1.Should().BeSameAs(result2, "results should be cached");
    }

    [Fact]
    public Task ContainsQuery_WithExistingSuccessQuery_ReturnsTrue()
    {
        // Arrange
        var successResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index"
        };

        var responseList = new List<MultiQueryResponse>
        {
            successResponse
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var contains = response.ContainsQuery("books");

        // Assert
        return Verify(contains);
    }

    [Fact]
    public Task ContainsQuery_WithExistingErrorQuery_ReturnsTrue()
    {
        // Arrange
        var errorResponse = new MultiQueryResponseError
        {
            Name = "books",
            Index = "books-index"
        };

        var responseList = new List<MultiQueryResponse>
        {
            errorResponse
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var contains = response.ContainsQuery("books");

        // Assert
        return Verify(contains);
    }

    [Fact]
    public Task ContainsQuery_WithNonExistingQuery_ReturnsFalse()
    {
        // Arrange
        var responseList = new List<MultiQueryResponse>();
        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var contains = response.ContainsQuery("nonexistent");

        // Assert
        return Verify(contains);
    }

    [Fact]
    public Task GetQueryNames_WithMultipleQueries_ReturnsAllNames()
    {
        // Arrange
        var responseList = new List<MultiQueryResponse>
        {
            new MultiQueryResponseSuccess
            {
                Name = "books", Index = "books-index"
            },
            new MultiQueryResponseSuccess
            {
                Name = "authors", Index = "authors-index"
            },
            new MultiQueryResponseError
            {
                Name = "categories", Index = "categories-index"
            }
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var names = response.GetQueryNames();

        // Assert
        return Verify(names);
    }

    [Fact]
    public Task GetQueryNames_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var response = new MultiQueryApiResponse
        {
            Response = null
        };

        // Act
        var names = response.GetQueryNames();

        // Assert
        return Verify(names);
    }

    [Fact]
    public Task Get_WithFacets_ReturnsFacetsInSuccess()
    {
        // Arrange
        var successResponse = new MultiQueryResponseSuccess
        {
            Name = "books",
            Index = "books-index",
            TotalResults = 1,
            Results = new List<Dictionary<string, object>>
            {
                new ()
                {
                    {
                        "title", "Test"
                    }
                }
            },
            Facets = new List<FacetResult>
            {
                new ()
                {
                    Name = "year",
                    Groups = new List<FacetGroup>
                    {
                        new ()
                        {
                            Value = "2021", Count = 5
                        },
                        new ()
                        {
                            Value = "2022", Count = 3
                        }
                    }
                }
            }
        };

        var responseList = new List<MultiQueryResponse>
        {
            successResponse
        };

        var response = new MultiQueryApiResponse
        {
            Response = responseList.ToMultiQueryResponse()
        };

        // Act
        var result = response.Get<TestBook>("books");

        // Assert
        return Verify(result);
    }

    #region End-to-End Flow Validation Tests

    [Fact]
    public async Task CompleteFlow_BuildQuery_MockCall_ValidateTypedResults()
    {
        // This test validates the complete flow:
        // 1. Build a multi-query using the fluent builder
        // 2. Make the API call with mocked HTTP response
        // 3. Validate the typed results can be retrieved correctly

        // STEP 1: Build multi-query request using fluent builder
        var request = new MultiQueryBuilder()
            .AddQuery("books", "book-index", builder => builder
                .Where(f => f.Equals("category", "fiction"))
                .WithPagination(0, 10))
            .AddQuery("authors", "author-index", builder => builder
                .Where(f => f.GreaterThan("bookCount", "5"))
                .WithPagination(0, 5))
            .Build();

        // Validate request structure
        request.Should().NotBeNull();
        request.Queries.Should().HaveCount(2);
        request.Queries.Should().Contain(q => q.Name == "books" && q.Index == "book-index");
        request.Queries.Should().Contain(q => q.Name == "authors" && q.Index == "author-index");

        // STEP 2: Mock the API HTTP call
        var serializer = new SystemTextJsonSerializer();
        var mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Create realistic API response matching Enterspeed's multi-query format
        var apiResponse = new List<object>
        {
            new
            {
                index = "book-index",
                name = "books",
                totalResults = 2,
                status = 0,  // Success
                results = new[]
                {
                    new
                    {
                        title = "The Great Novel",
                        author = "Jane Doe",
                        year = 2023
                    },
                    new
                    {
                        title = "Another Story",
                        author = "John Smith",
                        year = 2024
                    }
                },
                facets = Array.Empty<object>()
            },
            new
            {
                index = "author-index",
                name = "authors",
                totalResults = 1,
                status = 0,  // Success
                results = new[]
                {
                    new
                    {
                        name = "Jane Doe",
                        country = "USA"
                    }
                },
                facets = Array.Empty<object>()
            }
        };

        var responseJson = serializer.Serialize(apiResponse);

        mockHttpHandler
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

        // Setup service with mocked HTTP client
        var config = new EnterspeedQueryConfiguration();
        var configProvider = new EnterspeedQueryConfigurationProvider(config);
        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri(config.BaseUrl)
        };

        var queryService = new EnterspeedQueryService(httpClient, configProvider, serializer);

        // Execute the query
        var response = await queryService.Query("test-api-key", request.Queries.ToList(), CancellationToken.None);

        // STEP 3: Validate the API response structure
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Response.Should().NotBeNull();

        // STEP 4: Validate typed results retrieval using the fluent API
        var booksResponse = response.Get<TestBook>("books");
        booksResponse.Should().NotBeNull();
        booksResponse.Status.Should().BeTrue("books query should succeed");

        // Validate ISuccess<T> interface
        var booksSuccess = booksResponse as ISuccess<TestBook>;
        booksSuccess.Should().NotBeNull();
        booksSuccess?.TotalResults.Should().Be(2);
        booksSuccess?.Results.Should().HaveCount(2);

        // Validate strongly-typed data
        booksSuccess?.Results[0].Title.Should().Be("The Great Novel");
        booksSuccess?.Results[0].Author.Should().Be("Jane Doe");
        booksSuccess?.Results[0].Year.Should().Be(2023);

        booksSuccess?.Results[1].Title.Should().Be("Another Story");
        booksSuccess?.Results[1].Author.Should().Be("John Smith");
        booksSuccess?.Results[1].Year.Should().Be(2024);

        // Validate second query
        var authorsResponse = response.Get<TestAuthor>("authors");
        authorsResponse.Should().NotBeNull();
        authorsResponse.Status.Should().BeTrue("authors query should succeed");

        var authorsSuccess = authorsResponse as ISuccess<TestAuthor>;
        authorsSuccess.Should().NotBeNull();
        authorsSuccess?.TotalResults.Should().Be(1);
        authorsSuccess?.Results.Should().HaveCount(1);
        authorsSuccess?.Results[0].Name.Should().Be("Jane Doe");
        authorsSuccess?.Results[0].Country.Should().Be("USA");

        // Validate caching - subsequent calls should return same instance
        var booksResponseCached = response.Get<TestBook>("books");
        booksResponseCached.Should().BeSameAs(booksResponse, "responses should be cached");

        // Validate helper methods
        response.ContainsQuery("books").Should().BeTrue();
        response.ContainsQuery("authors").Should().BeTrue();
        response.ContainsQuery("nonexistent").Should().BeFalse();

        var queryNames = response.GetQueryNames();
        queryNames.Should().HaveCount(2);
        queryNames.Should().Contain("books");
        queryNames.Should().Contain("authors");
    }

    [Fact]
    public async Task CompleteFlow_WithPartialFailure_ValidatesIndependentResults()
    {
        // Validates that one failing query doesn't affect other successful queries

        // STEP 1: Build request
        var request = new MultiQueryBuilder()
            .AddQuery("valid-query", "valid-index", builder => builder.WithPagination(0, 10))
            .AddQuery("invalid-query", "invalid-index", builder => builder.WithPagination(0, 10))
            .Build();

        // STEP 2: Mock API with one success, one error
        var serializer = new SystemTextJsonSerializer();
        var mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var apiResponse = new List<object>
        {
            new
            {
                index = "valid-index",
                name = "valid-query",
                totalResults = 1,
                status = 0,  // Success
                results = new[]
                {
                    new
                    {
                        title = "Valid Book", author = "Valid Author", year = 2024
                    }
                },
                facets = Array.Empty<object>()
            },
            new
            {
                index = "invalid-index",
                name = "invalid-query",
                status = 1,  // Error
                message = "Index not found",
                errors = new[]
                {
                    "The specified index does not exist"
                }
            }
        };

        var responseJson = serializer.Serialize(apiResponse);

        mockHttpHandler
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

        // Setup service
        var config = new EnterspeedQueryConfiguration();
        var configProvider = new EnterspeedQueryConfigurationProvider(config);
        var httpClient = new HttpClient(mockHttpHandler.Object)
        {
            BaseAddress = new Uri(config.BaseUrl)
        };

        var queryService = new EnterspeedQueryService(httpClient, configProvider, serializer);

        // Execute
        var response = await queryService.Query("test-api-key", request.Queries.ToList(), CancellationToken.None);

        // STEP 3: Validate independent results
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Valid query should succeed
        var validResponse = response.Get<TestBook>("valid-query");
        validResponse.Status.Should().BeTrue();
        var validSuccess = validResponse as ISuccess<TestBook>;
        validSuccess?.Results.Should().HaveCount(1);
        validSuccess?.Results[0].Title.Should().Be("Valid Book");

        // Invalid query should fail with proper error details
        var invalidResponse = response.Get<TestBook>("invalid-query");
        invalidResponse.Status.Should().BeFalse();
        var invalidFailure = invalidResponse as ErrorResponse<TestBook>;
        invalidFailure.Should().NotBeNull();
        invalidFailure?.Errors.Should().NotBeEmpty();
        invalidFailure?.Errors.Should().Contain(e => e.Errors.Contains("The specified index does not exist"));
    }

    #endregion
}
