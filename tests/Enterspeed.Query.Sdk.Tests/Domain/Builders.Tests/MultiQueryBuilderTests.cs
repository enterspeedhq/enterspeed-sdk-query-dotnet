namespace Enterspeed.Query.Sdk.Tests.Domain.Builders.Tests;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Builders.Query;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using FluentAssertions;
using Xunit;
using static VerifyXunit.Verifier;

public class MultiQueryBuilderTests
{
    #region Records for typed query tests
    private record User
    {
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        [JsonPropertyName("department")]
        public string Department { get; set; }
    }

    private record Product
    {
        public string Name { get; set; }
        [JsonPropertyName("price")]

        public decimal Price { get; set; }
        public bool Active { get; set; }
        public string Department { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    #endregion

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
            .AddQuery<User>("users", "user-index", q => q.WithPagination(0, 10))
            .AddQuery<Product>("products", "product-index", q => q.WithPagination(0, 5))
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

    [Fact]
    public void AddQueryWithType_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var builder = new MultiQueryBuilder();

        // Act
        builder.AddQuery<Product>("products", "product-index", q => q
            .Where(filter => filter
                .Equals(p => p.Active, true)));

        Action act = () => builder.AddQuery<Product>("products", "product-index", q => q
            .Where(filter => filter
                .GreaterThan(p => p.Price, 100m)));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("A query with the key 'products' has already been added. Each query must have a unique key. (Parameter 'key')");
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
    public Task AddQuery_WithTypedFilters_BuildsCorrectQuery()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery<Product>("products", "product-index", q => q
            .WithPagination(0, 20)
            .SortBy(p => p.UpdatedAt, SortOrder.Desc)
            .Where(filter => filter
                .Equals(p => p.Active, true)));

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
    public Task Build_WithPagination_BuildsExpectedRequest()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q.WithPagination(2, 50));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_WithSorting_BuildsExpectedRequest()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q
            .SortBy("name", SortOrder.Asc)
            .SortBy("createdAt", SortOrder.Desc));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_WithFilters_BuildsExpectedRequest()
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
    public Task Build_WithFacets_BuildsExpectedRequest()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q
            .WithFacet("category", "categoryFacet", 25));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_WithAliases_BuildsExpectedRequest()
    {
        var builder = new MultiQueryBuilder();
        builder.AddQuery("test", "test-index", q => q
            .WithAliases("view1", "view2"));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_WithMultipleQueries_BuildsExpectedRequest()
    {
        var builder = new MultiQueryBuilder();
        builder
            .AddQuery("users", "user-index", q => q
                .WithPagination(0, 10)
                .SortBy("updatedAt", SortOrder.Desc)
                .Where(new EqualsOperator<bool>
                {
                    Field = "active",
                    Value = true
                })
                .WithFacet("department"))
            .AddQuery("products", "product-index", q => q
                .WithPagination(0, 20)
                .SortBy("price", SortOrder.Asc)
                .WithAliases("tile", "detail"));
        var request = builder.Build();
        return Verify(request);
    }

    [Fact]
    public Task Build_WithTypedQueries_BuildsExpectedRequest()
    {
        var builder = new MultiQueryBuilder();
        builder
            .AddQuery<User>("users", "user-index", q => q
                .WithPagination(page: 0, pageSize: 10)
                .SortBy(x => x.UpdatedAt, SortOrder.Desc)
                .Where(x => x.Equals(user => user.Active, true))
                .WithFacet(x => x.Department))
            .AddQuery<Product>("products", "product-index", q => q
                .WithPagination(page: 0, pageSize: 20)
                .SortBy(x => x.Price, SortOrder.Asc)
                .WithAliases("title", "detail"));
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
}
