namespace Enterspeed.Query.Sdk.Tests.Domain.SystemTextJson.Tests;

using System.Collections.Generic;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using FluentAssertions;
using Xunit;

public class QuerySearchSerializationTests
{
    private readonly SystemTextJsonSerializer _serializer = new ();

    [Fact]
    public void Serialize_WithSearch_WritesSearchMember()
    {
        var request = new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Search = new QuerySearch { Field = "title", Value = "hiking", Literal = false }
            })
            .Build();

        var json = _serializer.Serialize(request);

        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":false}");
    }

    [Fact]
    public void Serialize_WithLiteralSearch_WritesLiteralTrue()
    {
        var request = new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Search = new QuerySearch { Field = "title", Value = "hiking", Literal = true }
            })
            .Build();

        var json = _serializer.Serialize(request);

        json.Should().Contain("\"literal\":true");
    }

    [Fact]
    public void Serialize_WithoutSearch_OmitsSearchMember()
    {
        var request = new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            })
            .Build();

        var json = _serializer.Serialize(request);

        json.Should().NotContain("search");
    }

    /// <summary>
    /// Pins the exact bytes of a request that does not use search, so that adding the
    /// search member cannot change the wire format of existing callers.
    /// </summary>
    [Fact]
    public void Serialize_WithoutSearch_ProducesUnchangedWireFormat()
    {
        var request = new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            })
            .Build();

        var json = _serializer.Serialize(request);

        json.Should().Be(
            "{\"queries\":[{\"index\":\"blogpost\",\"name\":\"q1\",\"filters\":null,\"aliases\":null," +
            "\"sort\":null,\"pagination\":{\"page\":0,\"pageSize\":10},\"facets\":null}]}");
    }

    [Fact]
    public void Serialize_SearchWithScoreSort_WritesBothMembers()
    {
        var request = new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Search = new QuerySearch { Field = "title", Value = "hiking" },
                Sort = new List<Sort> { new () { Field = "_score", Order = SortOrder.Desc } }
            })
            .Build();

        var json = _serializer.Serialize(request);

        json.Should().Contain("\"sort\":[{\"field\":\"_score\",\"order\":\"desc\"}]");
        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":false}");
    }

    [Fact]
    public void Deserialize_WithSearch_RoundTripsAllMembers()
    {
        const string json = "{\"field\":\"title\",\"value\":\"hiking\",\"literal\":true}";

        var search = _serializer.Deserialize<QuerySearch>(json);

        search.Field.Should().Be("title");
        search.Value.Should().Be("hiking");
        search.Literal.Should().BeTrue();
    }
}
