using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Configuration;
using Enterspeed.Query.Sdk.Domain.Connection;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Services;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Enterspeed.Query.Sdk.Tests;

public class EnterspeedQueryServiceTests
{
    private readonly IEnterspeedQueryConfigurationProvider _configurationProvider;
    private readonly IJsonSerializer _serializer;
    private readonly EnterspeedQueryService _queryService;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly EnterspeedQueryConnection _queryConnection;

    public EnterspeedQueryServiceTests()
    {
        _serializer = new SystemTextJsonSerializer();
        var enterspeedQueryConfiguration = new EnterspeedQueryConfiguration();
        _configurationProvider = new EnterspeedQueryConfigurationProvider(enterspeedQueryConfiguration);

        // Mock the HttpMessageHandler
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        // Create real HttpClient with mocked handler
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(enterspeedQueryConfiguration.BaseUrl)
        };

        // Create real EnterspeedQueryConnection (sealed, can't mock)
        _queryConnection = new EnterspeedQueryConnection(_configurationProvider);

        // Use reflection to inject the mocked HttpClient BEFORE Connect() is called
        var httpClientField = typeof(EnterspeedQueryConnection)
            .GetField("_httpClientConnection",
                BindingFlags.NonPublic | BindingFlags.Instance);

        var connectionEstablishedDateField = typeof(EnterspeedQueryConnection)
            .GetField("_connectionEstablishedDate",
                BindingFlags.NonPublic | BindingFlags.Instance);

        // Inject the mocked HttpClient
        httpClientField?.SetValue(_queryConnection, httpClient);

        // Set connection established date to prevent re-connection
        connectionEstablishedDateField?.SetValue(_queryConnection, DateTime.Now);

        // Create real EnterspeedQueryService
        _queryService = new EnterspeedQueryService(_queryConnection, _configurationProvider, _serializer);
    }

    [Fact]
    public async Task Query_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var mockResponse = new QueryResponseSuccess
        {
            TotalResults = 2,
            Results = new List<Dictionary<string, object>>
            {
                new() { { "id", "1" }, { "name", "Item 1" } },
                new() { { "id", "2" }, { "website", "Item 2" } }
            },
            Facets = new List<FacetResult>
            {
                new() {
                    Name = "Categories", Field = "categories", IsValid = true,
                    Groups = new List<FacetGroup>{new() { Value = "Caps", Count = 8}, new() { Value = "Hoodies", Count = 2} }
                }
            }
        };

        var responseJson = _serializer.Serialize(mockResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            })
            .Verifiable();

        var query = new QueryObject
        {
            Sort = new List<Sort>{ new() { Field = "_updatedAt", Order = SortOrder.Desc }},
            Pagination = new Pagination { Page = 0, PageSize = 10 }
        };

        // Act
        var result = await _queryService.Query("environment-guid", "testIndex", query, CancellationToken.None);
        // // Assert
        // Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        // Assert.Equal(2, result.Response.TotalResults);
        // Assert.Equal(2, result.Response.Results.Count);
        if (result.Response is QueryResponseSuccess qResults)
        {
            Assert.Equal(2, qResults.TotalResults);
            Assert.Equal(2, qResults.Results.Count);
        }


        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task Query_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var mockResponse = new Dictionary<string, object>
        {
            { "message", "Query is not valid" },
            { "errors", new List<string> { "Field not found: title" } },
            { "status", 1 }
        };

        var responseJson = _serializer.Serialize(mockResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            })
            .Verifiable();

        var query = new QueryObject
        {
            Sort = new List<Sort>{ new() { Field = "_updatedAt", Order = SortOrder.Desc }},
            Pagination = new Pagination { Page = 0, PageSize = 10 }
        };

        // Act
        var result = await _queryService.Query("environment-guid", "testIndex", query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Query is not valid", result.Message );
        // Assert.Null(result.Response?.Results);
        // Assert.Null(result.Response?.Facets);
        // Assert.Null(result.Response?.TotalResults);

        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task MultiQuery_WithMultipleQueries_ReturnsSuccessfulResponses()
    {
        // Arrange
        var dictResponse = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Index", "index1"},
                { "Name", "query1" },
                { "TotalResults", 3 },
                { "Results", new List<Dictionary<string, object>>
                    {
                        new() { { "id", "1" }, { "title", "Post 1" }, {"te", "Post 3"} }
                    }
                },
                { "status", 0 }
            },
            new()
            {
                { "Index", "index2" },
                { "Name", "query2" },
                { "TotalResults", 2 },
                {
                    "Results", new List<Dictionary<string, object>>
                    {
                        new() { { "id", "2" }, { "title", "Post 2" } }
                    }
                },
                { "status", 0  }
            }
        };

        var responseJson = _serializer.Serialize(dictResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var queries = new List<MultiQueryObject>
        {
            new()
            {
                Index = "index1",
                Name = "query1",
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            },
            new()
            {
                Index = "index2",
                Name = "query2",
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            }
        };

        // Act
        var result = await _queryService.Query("environment-guid", queries);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(2, result.Response.GetSuccessResponse().Count);

        // if (result.Response is MultiQueryResponseSuccess queryResponse)
        // {
        //     queryResponse.Results.
        //     var query1Result = result.Response.First(x => x.Name == "query1");
        //     Assert.Equal(5, queryResponse.TotalResults);
        //     //Assert.Equal(0, query1Result.Status);
        // }
        //
        //
        // var query2Result = result.Response.First(x => x.Name == "query2");
        // Assert.Equal(3, query2Result.TotalResults);
        // //Assert.Equal(0, query2Result.Status);

        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task Query_WithErrorResponse_ReturnsErrorMessage()
    {
        // Arrange
        var errorResponses = new List<Dictionary<string, object>>
        {
            new()
            {
                { "Index", "index1" },
                { "Name", "query1" },
                { "status", 1 }
            },
            new()
            {
                { "Index", "index2" },
                { "Name", "query2" },
                { "status", 1 }
            }
        };


        var responseJson = _serializer.Serialize(errorResponses);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var queries = new List<MultiQueryObject>
        {
            new()
            {
                Index = "invalidIndex",
                Name = "query1",
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            }
        };

        // Act
        var result = await _queryService.Query("environment-guid", queries);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        //Assert.NotNull(result.Errors);
        //Assert.Contains("Index 'invalidIndex' does not exist", result.Errors);
    }


    [Fact]
    public async Task Query_WithMixedSuccessAndErrorResponse_ReturnsSuccessMessage()
    {
        // Arrange
        var responses = new List<Dictionary<string, object>>
        {
            new()
            {
                { "index", "index1" },
                { "name", "query1" },
                { "message", "Index is not found" },
                { "errors", new List<string> { "Index 'indexbase'" } },
                { "status", 1 }
            },
            new()
            {
                { "index", "index2" },
                { "name", "query2" },
                { "status", 1 }
            },
            new()
            {
                { "index", "index3" },
                { "name", "query3" },
                { "totalResults", 3 },
                {
                    "results", new List<Dictionary<string, object>>
                    {
                        new() { { "id", "1" }, { "title", "Post 1" }, { "te", "Post 3" } }
                    }
                },
                { "status", 0 }
            },
            new()
            {
                { "index", "index4" },
                { "name", "query4" },
                { "totalResults", 2 },
                {
                    "results", new List<Dictionary<string, object>>
                    {
                        new() { { "id", "2" }, { "title", "Post 2" } }
                    }
                },
                { "status", 0 }
            },
            new()
            {
                { "index", "Book" },
                { "name", "Book" },
                { "totalResults", 1 },
                {
                    "results", new List<Dictionary<string, object>>
                    {
                        new() { { "Title", "Book Title" } },
                        new () { { "Title", "Another Book Title" } }
                    }
                },
                { "status", 0 }
            }
        };



        var responseJson = _serializer.Serialize(responses);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        var queries = new List<MultiQueryObject>
        {
            new()
            {
                Index = "invalidIndex",
                Name = "query1",
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            },
            new()
            {
                Index = "validIndex",
                Name = "query2",
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            }
        };

        // Act
        var result = await _queryService.Query("environment-guid", queries);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        result.Response.GetSuccessResponse().Count.Should().Be(3);
        // TODO: TRy to get a element that is failed
        result.Response.TryGetResults<Book>("Book", out var res1);
        //var r= result.Response.GetResults<Book>()?.data;

        result.Response.TryGetResults<Dictionary<string, object>>("Book", out var res);

        // r is successInterface success
        // r.data
        // r.errors
        var testBook = new Book { Title = "Book Title" };
        var testBook2 = new Book { Title = "Another Book Title" };
        //result.Response.m

        res1[0].Should().BeEquivalentTo(testBook);
        res1[1].Should().BeEquivalentTo(testBook2);

        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    public class Book
    {
        // TODO: Use JsonPropertyName attribute to map JSON property to C# property correctly or we should make this easier in the serializer?
        [System.Text.Json.Serialization.JsonPropertyName("Title")]
        public string Title { get; set; }
    }
}
