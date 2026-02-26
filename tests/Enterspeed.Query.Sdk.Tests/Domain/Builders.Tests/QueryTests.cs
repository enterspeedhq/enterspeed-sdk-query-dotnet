namespace Enterspeed.Query.Sdk.Tests.Domain.Builders.Tests;

using System;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Domain.Builders.Filter;
using Enterspeed.Query.Sdk.Domain.Builders.Query;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using FluentAssertions;
using Xunit;
using static VerifyXunit.Verifier;

public class QueryTests
{
    [Fact]
    public Task Build_WithNoPagination_ReturnsQueryWithNullPagination()
    {
        var builder = new Query();
        var result = builder.Build();
        return Verify(result);
    }

    [Fact]
    public Task WithPagination_ValidValues_SetsPagination()
    {
        var builder = new Query();
        var result = builder
            .WithPagination(2, 50)
            .Build();
        return Verify(result);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(-5, 20)]
    public void WithPagination_NegativePage_ThrowsArgumentException(int page, int pageSize)
    {
        var builder = new Query();
        Action act = () => builder.WithPagination(page, pageSize);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Page number must be non-negative*");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, -1)]
    public void WithPagination_InvalidPageSize_ThrowsArgumentException(int page, int pageSize)
    {
        var builder = new Query();
        Action act = () => builder.WithPagination(page, pageSize);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Page size must be positive*");
    }

    [Fact]
    public Task SortBy_ValidField_AddsSortCriterion()
    {
        var builder = new Query();
        var result = builder
            .SortBy("name", SortOrder.Asc)
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task SortBy_MultipleCriteria_AddsAllSorts()
    {
        var builder = new Query();
        var result = builder
            .SortBy("name", SortOrder.Asc)
            .SortBy("createdAt", SortOrder.Desc)
            .Build();
        return Verify(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SortBy_NullOrEmptyField_ThrowsArgumentException(string field)
    {
        var builder = new Query();
        Action act = () => builder.SortBy(field);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Field name cannot be null or whitespace*");
    }

    [Fact]
    public Task Where_ValidFilter_AddsFilter()
    {
        var builder = new Query();
        var filter = new EqualsOperator<string>
        {
            Field = "status", Value = "active"
        };
        var result = builder
            .Where(filter)
            .Build();
        return Verify(result);
    }

    [Fact]
    public void Where_NullFilter_ThrowsArgumentNullException()
    {
        var builder = new Query();
        Action act = () => builder.Where((FilterOperator)null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public Task WhereAll_MultipleFilters_AddsAllFilters()
    {
        var builder = new Query();
        var filter1 = new EqualsOperator<string>
        {
            Field = "status", Value = "active"
        };
        var filter2 = new GreaterThanOperator<int>
        {
            Field = "age", Value = 18
        };
        var result = builder
            .WhereAll(filter1, filter2)
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task WhereAll_NoFilters_DoesNotThrow()
    {
        var builder = new Query();
        var result = builder.WhereAll().Build();
        return Verify(result);
    }

    [Fact]
    public Task WithFacet_ValidField_AddsFacet()
    {
        var builder = new Query();
        var result = builder
            .WithFacet("category", "categoryFacet", 20)
            .Build();
        return Verify(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithFacet_NullOrEmptyField_ThrowsArgumentException(string field)
    {
        var builder = new Query();
        Action act = () => builder.WithFacet(field);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Field name cannot be null or whitespace*");
    }

    [Fact]
    public Task WithAliases_ValidAliases_AddsAliases()
    {
        var builder = new Query();
        var result = builder
            .WithAliases("alias1", "alias2", "alias3")
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task WithAliases_NoAliases_DoesNotThrow()
    {
        var builder = new Query();
        var result = builder.WithAliases().Build();
        return Verify(result);
    }

    [Fact]
    public Task Build_CompleteQuery_ReturnsFullyConfiguredQuery()
    {
        var builder = new Query();
        var filter = new EqualsOperator<string>
        {
            Field = "status", Value = "active"
        };
        var result = builder
            .WithPagination(0, 10)
            .SortBy("name", SortOrder.Asc)
            .Where(filter)
            .WithFacet("category", size: 15)
            .WithAliases("detailView")
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Build_FluentChaining_AllowsMethodChaining()
    {
        var result = new Query()
            .WithPagination(0, 5)
            .SortBy("updatedAt", SortOrder.Desc)
            .Where(new EqualsOperator<bool>
            {
                Field = "active", Value = true
            })
            .Build();
        return Verify(result);
    }

    #region Lambda-Based Where() Tests

    [Fact]
    public Task Where_WithLambdaFilter_BuildsFilter()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.Equals("status", "active"))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndMultipleConditions_WrapsInAndOperator()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f
                .Equals("status", "active")
                .GreaterThan("age", 18))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndCaseInsensitive_SetsCaseInsensitiveProperty()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.Equals("status", "active", caseInsensitive: true))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndNestedOr_CreatesOrOperator()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f
                .Equals("title", "hoodie")
                .Or(o => o
                    .Equals("isInStock", true)
                    .Equals("allowPreorder", true)))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_MultipleLambdaCalls_AccumulatesWithAnd()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.Equals("status", "active"))
            .Where(f => f.GreaterThan("age", 18))
            .Where(f => f.NotEquals("role", "guest"))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_MixedLambdaAndPowerUserAPI_AccumulatesAll()
    {
        var builder = new Query();
        var powerUserFilter = new EqualsOperator<bool>
        {
            Field = "verified", Value = true
        };
        var result = builder
            .Where(f => f.Equals("status", "active"))
            .Where(powerUserFilter)
            .Where(f => f.GreaterThan("age", 18))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndEmptyFilter_DoesNotAddFilter()
    {
        var builder = new Query();
        var result = builder
            .Where(_ => { })
            .Build();
        return Verify(result);
    }

    [Fact]
    public void Where_WithNullLambda_ThrowsArgumentNullException()
    {
        var builder = new Query();
        Action act = () => builder.Where((Action<IFilterBuilder>)null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public Task Where_WithLambdaAndInOperator_AddsInOperator()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.In("category", "electronics", "computers", "phones"))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndContainsOperator_AddsContainsOperator()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.Contains("description", "*premium*", caseInsensitive: true))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_ComplexNestedLambda_BuildsCorrectStructure()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f
                .Equals("status", "active")
                .Or(o => o
                    .Equals("isInStock", true)
                    .Equals("allowPreorder", true))
                .And(a => a
                    .GreaterThanOrEquals("price", 10)
                    .LessThanOrEquals("price", 100)))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndPagination_CombinesBoth()
    {
        var builder = new Query();
        var result = builder
            .WithPagination(0, 10)
            .Where(f => f.Equals("status", "active"))
            .SortBy("name", SortOrder.Asc)
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndFacets_CombinesBoth()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.Equals("status", "active"))
            .WithFacet("category", size: 20)
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndAliases_CombinesBoth()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f.Equals("isFeatured", true))
            .WithAliases("tile", "detail")
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_MultipleLambdaCallsWithComplexFilters_BuildsCorrectStructure()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f
                .Equals("status", "active")
                .GreaterThan("age", 18))
            .Where(f => f.Or(o => o
                .Equals("role", "admin")
                .Equals("role", "moderator")))
            .Where(f => f.NotEquals("banned", true))
            .Build();
        return Verify(result);
    }

    [Fact]
    public Task Where_WithLambdaAndAllOperators_BuildsCorrectly()
    {
        var builder = new Query();
        var result = builder
            .Where(f => f
                .Equals("status", "active")
                .NotEquals("role", "guest")
                .GreaterThan("age", 18)
                .GreaterThanOrEquals("score", 75)
                .LessThan("failedAttempts", 3)
                .LessThanOrEquals("discount", 50)
                .Contains("description", "*premium*")
                .In("category", "electronics", "computers"))
            .Build();
        return Verify(result);
    }

    #endregion
}
