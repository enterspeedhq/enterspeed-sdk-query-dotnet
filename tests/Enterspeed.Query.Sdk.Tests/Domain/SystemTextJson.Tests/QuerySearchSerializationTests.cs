namespace Enterspeed.Query.Sdk.Tests.Domain.SystemTextJson.Tests;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Builders.Query;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using FluentAssertions;
using Xunit;

/// <summary>
/// Serializes the same payload the query service sends over the wire, which is the
/// bare queries array (see EnterspeedQueryService.Query), not the QueryRequest wrapper.
/// </summary>
public class QuerySearchSerializationTests
{
    private readonly SystemTextJsonSerializer _serializer = new ();

    private record Article
    {
        public string Title { get; init; }

        [JsonPropertyName("body_text")]
        public string Body { get; init; }
    }

    [Fact]
    public void Serialize_WithSearch_WritesSearchMember()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Search = new QuerySearch { Field = "title", Value = "hiking", Literal = false }
            }));

        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":false}");
    }

    [Fact]
    public void Serialize_WithLiteralSearch_WritesLiteralTrue()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Search = new QuerySearch { Field = "title", Value = "hiking", Literal = true }
            }));

        json.Should().Contain("\"literal\":true");
    }

    [Fact]
    public void Serialize_WithoutSearch_OmitsSearchMember()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            }));

        json.Should().NotContain("search");
    }

    /// <summary>
    /// Pins the exact bytes sent for a request that does not use search, so that adding
    /// the search member cannot change the wire format for existing callers.
    /// </summary>
    [Fact]
    public void Serialize_WithoutSearch_ProducesUnchangedWireFormat()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Pagination = new Pagination { Page = 0, PageSize = 10 }
            }));

        json.Should().Be(
            "[{\"index\":\"blogpost\",\"name\":\"q1\",\"filters\":null,\"aliases\":null," +
            "\"sort\":null,\"pagination\":{\"page\":0,\"pageSize\":10},\"facets\":null}]");
    }

    [Fact]
    public void Serialize_SearchWithScoreSort_WritesBothMembers()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery("q1", "blogpost", new QueryObject
            {
                Search = new QuerySearch { Field = "title", Value = "hiking" },
                Sort = new List<Sort> { new () { Field = "_score", Order = SortOrder.Desc } }
            }));

        json.Should().Contain("\"sort\":[{\"field\":\"_score\",\"order\":\"desc\"}]");
        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":false}");
    }

    [Fact]
    public void Serialize_NonGenericFluentBuilder_WritesSearchMember()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery("q1", "blogpost", q => q.WithSearch("title", "hiking")));

        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":false}");
    }

    [Fact]
    public void Serialize_GenericFluentBuilderWithStringField_WritesSearchMember()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery<Article>("q1", "blogpost", q => q.WithSearch("title", "hiking", true)));

        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":true}");
    }

    [Fact]
    public void Serialize_GenericFluentBuilderWithPropertySelector_CamelCasesFieldName()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery<Article>("q1", "blogpost", q => q.WithSearch(x => x.Title, "hiking")));

        json.Should().Contain("\"search\":{\"field\":\"title\",\"value\":\"hiking\",\"literal\":false}");
    }

    [Fact]
    public void Serialize_GenericFluentBuilderWithPropertySelector_HonoursJsonPropertyName()
    {
        var json = SerializeBody(new QueryBuilder()
            .AddQuery<Article>("q1", "blogpost", q => q.WithSearch(x => x.Body, "camping", true)));

        json.Should().Contain("\"search\":{\"field\":\"body_text\",\"value\":\"camping\",\"literal\":true}");
    }

    [Fact]
    public void Serialize_GenericBuilderViaNonGenericInterface_WritesSearchMember()
    {
        IQuery query = new Query<Article>();
        var queryObject = query.WithSearch("title", "hiking").Build();

        var json = SerializeBody(new QueryBuilder().AddQuery("q1", "blogpost", queryObject));

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

    private string SerializeBody(QueryBuilder builder)
    {
        return _serializer.Serialize(builder.Build().Queries);
    }
}
