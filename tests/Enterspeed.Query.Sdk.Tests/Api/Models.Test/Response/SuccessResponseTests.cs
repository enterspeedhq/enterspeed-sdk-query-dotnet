using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

namespace Enterspeed.Query.Sdk.Tests.Api.Models.Test.Response;

using Enterspeed.Query.Sdk.Domain.QueryApiResponse;

using System.Collections.Generic;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Models.Response;
using FluentAssertions;
using Xunit;
using static VerifyXunit.Verifier;

public class SuccessResponseTests
{
    private class TestData
    {
        public string Value { get; set; }
    }

    [Fact]
    public Task Constructor_WithResults_InitializesProperties()
    {
        var results = new List<TestData>
        {
            new ()
            {
                Value = "Test1"
            },
            new ()
            {
                Value = "Test2"
            }
        };
        var facets = new List<FacetResult>
        {
            new ()
            {
                Name = "category"
            }
        };

        var response = new SuccessResponse<TestData>(new QueryResponseSuccess<TestData>
        {
            Results = results,
            TotalResults = 10,
            Facets = facets,
        });

        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithNullResults_InitializesEmptyList()
    {
        var response = new SuccessResponse<TestData>(null, 0);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithNullFacets_InitializesEmptyList()
    {
        var results = new List<TestData>
        {
            new ()
                {
                    Value = "Test"
                }
        };
        var response = new SuccessResponse<TestData>(results, 1);
        return Verify(response);
    }

    [Fact]
    public void Status_AlwaysReturnsTrue()
    {
        var response = new SuccessResponse<TestData>(new List<TestData>(), 0);
        response.Status.Should().BeTrue();
    }

    [Fact]
    public Task Value_WithResults_ReturnsFirstResult()
    {
        var results = new List<TestData>
        {
            new ()
            {
                Value = "First"
            },
            new ()
            {
                Value = "Second"
            }
        };
        var response = new SuccessResponse<TestData>(results, 2);
        var first = response.Value();
        return Verify(first);
    }

    [Fact]
    public void Value_WithEmptyResults_ReturnsNull()
    {
        var response = new SuccessResponse<TestData>(new List<TestData>(), 0);
        var first = response.Value();
        first.Should().BeNull();
    }

    [Fact]
    public void Results_ReturnsReadOnlyList()
    {
        var results = new List<TestData>
        {
            new ()
            {
                Value = "Test"
            }
        };
        var response = new SuccessResponse<TestData>(results, 1);
        var resultsList = response.Results;
        resultsList.Should().BeAssignableTo<IReadOnlyList<TestData>>();
    }

    [Fact]
    public void Facets_ReturnsReadOnlyList()
    {
        var facets = new List<FacetResult>
        {
            new ()
            {
                Name = "test"
            }
        };
        var response = new SuccessResponse<TestData>(new List<TestData>(), 0, facets);
        var facetsList = response.Facets;
        facetsList.Should().BeAssignableTo<IReadOnlyList<FacetResult>>();
    }

    [Fact]
    public void ImplementsISuccess()
    {
        var response = new SuccessResponse<TestData>(new List<TestData>(), 0);
        response.Should().BeAssignableTo<ISuccess<TestData>>();
        response.Should().BeAssignableTo<IResponse<TestData>>();
        response.Should().BeAssignableTo<IResponse>();
    }
}
