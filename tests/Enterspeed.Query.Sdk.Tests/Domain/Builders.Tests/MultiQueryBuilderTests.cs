namespace Enterspeed.Query.Sdk.Tests.Domain.Builders.Tests;

using System;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Domain.Builders;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using FluentAssertions;
using Xunit;
using static VerifyXunit.Verifier;

public class MultiQueryBuilderTests
{
    #region Query Construction Tests

    [Fact]
    public Task AddQuery_WithFluentBuilder_AddsQuerySuccessfully()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("users", "user-index", q => q
            .WithPagination(0, 10)
            .SortBy("name", SortOrder.Asc));
        var result = builder.Build();
        return Verify(result);
    }

    [Fact]
    public Task AddQuery_WithPreConstructedQuery_AddsQuerySuccessfully()
    {
        var builder = new MultiQueryBuilder();
        var query = new QueryObject
        {
            Pagination = new Pagination
            {
                Page = 0, PageSize = 5
            }
        };
        builder.AddQuery("products", "product-index", query);
        var result = builder.Build();
        return Verify(result);
    }

    [Fact]
    public Task AddQuery_MultipleQueries_AddsAllWithUniqueKeys()
    {
        var builder = new MultiQueryBuilder();
        builder
            .AddQuery("users", "user-index", q => q.WithPagination(0, 10))
            .AddQuery("products", "product-index", q => q.WithPagination(0, 5))
            .AddQuery("orders", "order-index", new QueryObject());
        var result = builder.Build();
        return Verify(result);
    }

    [Fact]
    public void AddQuery_DuplicateKey_ThrowsArgumentException()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("users", "user-index", q => q.WithPagination(0, 10));
        Action act = () => builder.AddQuery("users", "another-index", q => q.WithPagination(0, 5));
        act.Should().Throw<ArgumentException>()
            .WithMessage("*key 'users' has already been added*")
            .And.ParamName.Should().Be("key");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddQuery_NullOrEmptyKey_ThrowsArgumentException(string key)
    {
        var builder = new MultiQueryBuilder();
        Action act = () => builder.AddQuery(key, "index", _ => { });
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Query key cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddQuery_NullOrEmptyIndex_ThrowsArgumentException(string index)
    {
        var builder = new MultiQueryBuilder();
        Action act = () => builder.AddQuery("key", index, _ => { });
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Index cannot be null or empty*");
    }

    [Fact]
    public void AddQuery_NullBuilderAction_ThrowsArgumentNullException()
    {
        var builder = new MultiQueryBuilder();
        Action act = () => builder.AddQuery("key", "index", (Action<IQueryBuilder>)null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQuery_NullQueryObject_ThrowsArgumentNullException()
    {
        var builder = new MultiQueryBuilder();
        Action act = () => builder.AddQuery("key", "index", (QueryObject)null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddQuery_ExceedsMaximumQueries_ThrowsInvalidOperationException()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("q1", "index", _ => { });
        builder.AddQuery("q2", "index", _ => { });
        builder.AddQuery("q3", "index", _ => { });
        builder.AddQuery("q4", "index", _ => { });
        builder.AddQuery("q5", "index", _ => { });
        Action act = () => builder.AddQuery("q6", "index", q => { });
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot add more than 5 queries*");
    }

    #endregion

    #region Build Tests

    [Fact]
    public Task Build_WithValidQueries_ReturnsImmutableRequest()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("users", "user-index", q => q.WithPagination(0, 10));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public void Build_NoQueries_ThrowsInvalidOperationException()
    {
        var builder = new MultiQueryBuilder();
        Action act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot build a multi-query request with no queries*");
    }

    [Fact]
    public Task Build_AfterBuild_OriginalBuilderIsNotAffected()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("users", "user-index", _ => { });
        var request1 = builder.Build();
        builder.AddQuery("products", "product-index", _ => { });
        var request2 = builder.Build();
        return Verify(new
        {
            request1, request2
        });
    }

    #endregion

    #region Serialization & API Contract Tests

    [Fact]
    public Task Build_QueryWithPagination_SerializesCorrectly()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q.WithPagination(2, 50));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_QueryWithSorting_SerializesCorrectly()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q
            .SortBy("name", SortOrder.Asc)
            .SortBy("createdAt", SortOrder.Desc));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_QueryWithFilters_SerializesCorrectly()
    {
        var builder = new MultiQueryBuilder();
        var filter = new EqualsOperator<string>
        {
            Field = "status", Value = "active"
        };
        builder.AddQuery("test", "test-index", q => q.Where(filter));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_QueryWithFacets_SerializesCorrectly()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q
            .WithFacet("category", "categoryFacet", 25));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_QueryWithAliases_SerializesCorrectly()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q
            .WithAliases("view1", "view2"));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_ComplexMultiQuery_MatchesAPIContract()
    {
        var builder = new MultiQueryBuilder();
        builder
            .AddQuery("users", "user-index", q => q
                .WithPagination(0, 10)
                .SortBy("updatedAt", SortOrder.Desc)
                .Where(new EqualsOperator<bool>
                {
                    Field = "active", Value = true
                })
                .WithFacet("department"))
            .AddQuery("products", "product-index", q => q
                .WithPagination(0, 20)
                .SortBy("price", SortOrder.Asc)
                .WithAliases("tile", "detail"));
        var request = builder.Build();
        return Verify(request);
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("users", "user-index", _ => { });
        var result = builder.ContainsKey("users");
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsKey_NonExistingKey_ReturnsFalse()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("users", "user-index", _ => { });
        var result = builder.ContainsKey("products");
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ContainsKey_NullOrEmptyKey_ReturnsFalse(string key)
    {
        var builder = new MultiQueryBuilder();
        var result = builder.ContainsKey(key);
        result.Should().BeFalse();
    }

    [Fact]
    public void Count_InitiallyZero()
    {
        var builder = new MultiQueryBuilder();
        builder.Count.Should().Be(0);
    }

    [Fact]
    public void Count_IncreasesWithEachQuery()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("q1", "index", _ => { });
        builder.AddQuery("q2", "index", _ => { });
        builder.AddQuery("q3", "index", _ => { });
        builder.Count.Should().Be(3);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public Task FluentAPI_CompleteScenario_BuildsCorrectRequest()
    {
        var request = new MultiQueryBuilder()
            .AddQuery("activeUsers", "user-index", builder => builder
                .WithPagination(0, 25)
                .SortBy("lastName", SortOrder.Asc)
                .SortBy("firstName", SortOrder.Asc)
                .Where(new EqualsOperator<bool>
                    {
                        Field = "isActive", Value = true
                    })
                .WithFacet("role", size: 10)
                .WithAliases("summary"))
            .AddQuery("recentOrders", "order-index", builder => builder
                .WithPagination(0, 50)
                .SortBy("orderDate", SortOrder.Desc)
                .WhereAll(
                    new GreaterThanOperator<string>
                    {
                        Field = "orderDate", Value = "2025-01-01"
                    },
                    new EqualsOperator<string>
                    {
                        Field = "status", Value = "completed"
                    }))
            .AddQuery("popularProducts", "product-index", new QueryObject
            {
                Pagination = new Pagination
                {
                    Page = 0, PageSize = 10
                },
                Sort = new System.Collections.Generic.List<Sort>
                {
                    new ()
                    {
                        Field = "viewCount", Order = SortOrder.Desc
                    }
                }
            })
            .Build();
        return Verify(request);
    }

    #endregion
}
