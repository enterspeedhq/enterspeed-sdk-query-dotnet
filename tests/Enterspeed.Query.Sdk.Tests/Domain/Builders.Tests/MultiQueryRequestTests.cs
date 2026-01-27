using System.Collections.Generic;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.Models;
using FluentAssertions;
using static VerifyXunit.Verifier;
using Xunit;

namespace Enterspeed.Query.Sdk.Tests.Domain.MultiQueriBuilder
{
    /// <summary>
    /// Tests for MultiQueryRequest immutability and structure.
    /// </summary>
    public class MultiQueryRequestTests
    {
        [Fact]
        public void Constructor_WithQueries_CreatesImmutableRequest()
        {
            // Arrange
            var queries = new List<MultiQueryObject>
            {
                new MultiQueryObject { Name = "test1", Index = "index1" },
                new MultiQueryObject { Name = "test2", Index = "index2" }
            };

            // Act
            var request = new MultiQueryRequest(queries);

            // Assert
            request.Should().NotBeNull();
            request.Queries.Should().HaveCount(2);
        }

        [Fact]
        public void Queries_IsReadOnly()
        {
            // Arrange
            var queries = new List<MultiQueryObject>
            {
                new MultiQueryObject { Name = "test", Index = "index" }
            };
            var request = new MultiQueryRequest(queries);

            // Act & Assert
            request.Queries.Should().BeAssignableTo<IReadOnlyList<MultiQueryObject>>();
        }

        [Fact]
        public void Queries_ChangingOriginalList_DoesNotAffectRequest()
        {
            // Arrange
            var queries = new List<MultiQueryObject>
            {
                new MultiQueryObject { Name = "test1", Index = "index1" }
            };
            var request = new MultiQueryRequest(queries);

            // Act
            queries.Add(new MultiQueryObject { Name = "test2", Index = "index2" });

            // Assert
            request.Queries.Should().HaveCount(1, "the request should be immutable");
        }

        [Fact]
        public void Queries_PreservesOrder()
        {
            // Arrange
            var queries = new List<MultiQueryObject>
            {
                new MultiQueryObject { Name = "first", Index = "index1" },
                new MultiQueryObject { Name = "second", Index = "index2" },
                new MultiQueryObject { Name = "third", Index = "index3" }
            };

            // Act
            var request = new MultiQueryRequest(queries);

            // Assert
            request.Queries[0].Name.Should().Be("first");
            request.Queries[1].Name.Should().Be("second");
            request.Queries[2].Name.Should().Be("third");
        }
    }
}
