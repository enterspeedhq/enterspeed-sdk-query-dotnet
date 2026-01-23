// using System.Collections.Generic;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using Enterspeed.Query.Sdk.Api.Models;
// using Enterspeed.Query.Sdk.Api.Providers;
// using Enterspeed.Query.Sdk.Api.Services;
// using Enterspeed.Query.Sdk.Configuration;
// using Enterspeed.Query.Sdk.Domain.Models;
// using Enterspeed.Query.Sdk.Domain.SystemTextJson;
// using Moq;
// using NSubstitute;
// using Xunit;
//
// namespace Enterspeed.Query.Sdk.Tests;
//
// public class QueryServiceTestss
// {
//     private readonly IEnterspeedQueryConfigurationProvider _configurationProvider;
//     private readonly IJsonSerializer _serializer;
//     private readonly Mock<IEnterspeedQueryService> _mockQueryService;
//     private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
//
//
//     public QueryServiceTestss()
//     {
//         _serializer = new SystemTextJsonSerializer();
//         var enterspeedQueryConfiguration = new EnterspeedQueryConfiguration();
//         _configurationProvider = new EnterspeedQueryConfigurationProvider(enterspeedQueryConfiguration);
//
//         // Only mock the interface, not the sealed class
//         _mockQueryService = new Mock<IEnterspeedQueryService>();
//         _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
//     }
//
//     [Fact]
//     public async Task Query_WithValidRequest_ReturnsSuccess()
//     {
//         // Arrange
//         var mockResponse = new QueryApiResponse
//         {
//             StatusCode = System.Net.HttpStatusCode.OK,
//             Response = new QueryResponse
//             {
//                 TotalResults = 1,
//                 Results = new List<Dictionary<string, object>>
//                 {
//                     new() { { "id", "1" }, { "name", "Test" } }
//                 }
//             }
//         };
//
//         _mockQueryService
//             .Setup(x => x.Query(
//                 It.IsAny<string>(),
//                 It.IsAny<string>(),
//                 It.IsAny<QueryObject>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(mockResponse);
//
//         // Act
//         var result = await _mockQueryService.Object.Query("env-guid", "index", new QueryObject(),  CancellationToken.None);
//         //_mockHttpMessageHandler.Object.Returns()
//
//         // Assert
//         Assert.NotNull(result);
//         Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
//         Assert.Single(result.Response.Results);
//     }
//
//     [Fact]
//     public async Task MultiQuery_WithValidQueries_ReturnsResponse()
//     {
//         // Arrange
//         var mockResponse = new MultiQueryApiResponse
//         {
//             StatusCode = System.Net.HttpStatusCode.OK,
//             Response = new List<MultiQueryResponse>
//             {
//                 new()
//                 {
//                     Index = "index1",
//                     Name = "query1",
//                     //Status = 0,
//                     TotalResults = 5
//                 }
//             }
//         };
//
//         _mockQueryService
//             .Setup(x => x.Query(
//                 It.IsAny<string>(),
//                 It.IsAny<List<MultiQueryObject>>(),
//                 It.IsAny<CancellationToken>()))
//             .ReturnsAsync(mockResponse);
//
//         var queries = new List<MultiQueryObject>
//         {
//             new() { Index = "index1", Name = "query1" }
//         };
//
//         // Act
//         var result = await _mockQueryService.Object.Query("env-guid", queries, CancellationToken.None);
//
//         // Assert
//         Assert.NotNull(result);
//         Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
//     }
// }
//
