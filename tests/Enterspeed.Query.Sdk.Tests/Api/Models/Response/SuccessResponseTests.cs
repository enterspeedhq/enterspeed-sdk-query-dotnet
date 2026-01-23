using System.Collections.Generic;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Models.Response;
using FluentAssertions;
using Xunit;

namespace Enterspeed.Query.Sdk.Tests.Api.Models.Response
{
    public class SuccessResponseTests
    {
        private class TestData
        {
            public string Value { get; set; }
        }

        [Fact]
        public void Constructor_WithResults_InitializesProperties()
        {
            // Arrange
            var results = new List<TestData>
            {
                new TestData { Value = "Test1" },
                new TestData { Value = "Test2" }
            };
            var facets = new List<FacetResult>
            {
                new FacetResult { Name = "category" }
            };

            // Act
            var response = new SuccessResponse<TestData>(results, 10, facets);

            // Assert
            response.Status.Should().BeTrue();
            response.Results.Should().HaveCount(2);
            response.TotalResults.Should().Be(10);
            response.Facets.Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_WithNullResults_InitializesEmptyList()
        {
            // Act
            var response = new SuccessResponse<TestData>(null, 0);

            // Assert
            response.Results.Should().BeEmpty();
            response.Results.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullFacets_InitializesEmptyList()
        {
            // Arrange
            var results = new List<TestData> { new TestData { Value = "Test" } };

            // Act
            var response = new SuccessResponse<TestData>(results, 1, null);

            // Assert
            response.Facets.Should().BeEmpty();
            response.Facets.Should().NotBeNull();
        }

        [Fact]
        public void Status_AlwaysReturnsTrue()
        {
            // Arrange
            var response = new SuccessResponse<TestData>(new List<TestData>(), 0);

            // Act & Assert
            response.Status.Should().BeTrue();
        }

        [Fact]
        public void Value_WithResults_ReturnsFirstResult()
        {
            // Arrange
            var results = new List<TestData>
            {
                new TestData { Value = "First" },
                new TestData { Value = "Second" }
            };
            var response = new SuccessResponse<TestData>(results, 2);

            // Act
            var first = response.Value();

            // Assert
            first.Should().NotBeNull();
            first.Value.Should().Be("First");
        }

        [Fact]
        public void Value_WithEmptyResults_ReturnsNull()
        {
            // Arrange
            var response = new SuccessResponse<TestData>(new List<TestData>(), 0);

            // Act
            var first = response.Value();

            // Assert
            first.Should().BeNull();
        }

        [Fact]
        public void Results_ReturnsReadOnlyList()
        {
            // Arrange
            var results = new List<TestData> { new TestData { Value = "Test" } };
            var response = new SuccessResponse<TestData>(results, 1);

            // Act
            var resultsList = response.Results;

            // Assert
            resultsList.Should().BeAssignableTo<IReadOnlyList<TestData>>();
        }

        [Fact]
        public void Facets_ReturnsReadOnlyList()
        {
            // Arrange
            var facets = new List<FacetResult> { new FacetResult { Name = "test" } };
            var response = new SuccessResponse<TestData>(new List<TestData>(), 0, facets);

            // Act
            var facetsList = response.Facets;

            // Assert
            facetsList.Should().BeAssignableTo<IReadOnlyList<FacetResult>>();
        }

        [Fact]
        public void ImplementsISuccess()
        {
            // Arrange
            var response = new SuccessResponse<TestData>(new List<TestData>(), 0);

            // Assert
            response.Should().BeAssignableTo<ISuccess<TestData>>();
            response.Should().BeAssignableTo<IResponse<TestData>>();
            response.Should().BeAssignableTo<IResponse>();
        }
    }
}
