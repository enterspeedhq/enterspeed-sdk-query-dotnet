namespace Enterspeed.Query.Sdk.Tests.Domain.Builders.Tests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.Models;
using FluentAssertions;
using static VerifyXunit.Verifier;
using Xunit;

public class MultiQueryRequestTests
{
    [Fact]
    public Task Constructor_WithQueries_CreatesImmutableRequest()
    {
        var queries = new List<MultiQueryObject>
        {
            new ()
            {
                Name = "test1", Index = "index1"
            },
            new ()
            {
                Name = "test2", Index = "index2"
            }
        };
        var request = new MultiQueryRequest(queries);
        return Verify(request);
    }

    [Fact]
    public void Queries_IsReadOnly()
    {
        var queries = new List<MultiQueryObject>
        {
            new ()
            {
                Name = "test", Index = "index"
            }
        };
        var request = new MultiQueryRequest(queries);
        request.Queries.Should().BeAssignableTo<IReadOnlyList<MultiQueryObject>>();
    }

    [Fact]
    public Task Queries_ChangingOriginalList_DoesNotAffectRequest()
    {
        var queries = new List<MultiQueryObject>
        {
            new ()
            {
                Name = "test1", Index = "index1"
            }
        };
        var request = new MultiQueryRequest(queries);
        queries.Add(new MultiQueryObject { Name = "test2", Index = "index2" });
        return Verify(request)
            .UseMethodName("Queries_ChangingOriginalList_DoesNotAffectRequest_StillHasOne");
    }

    [Fact]
    public Task Queries_PreservesOrder()
    {
        var queries = new List<MultiQueryObject>
        {
            new ()
            {
                Name = "first", Index = "index1"
            },
            new ()
            {
                Name = "second", Index = "index2"
            },
            new ()
            {
                Name = "third", Index = "index3"
            }
        };
        var request = new MultiQueryRequest(queries);
        return Verify(request);
    }
}
