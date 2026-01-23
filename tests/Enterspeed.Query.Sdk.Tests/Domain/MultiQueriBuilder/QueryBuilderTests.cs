using System;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;
using Enterspeed.Query.Sdk.Domain.MultiQueriBuilder;
using FluentAssertions;
using Xunit;

namespace Enterspeed.Query.Sdk.Tests.Domain.MultiQueriBuilder
{
    /// <summary>
    /// Tests for the QueryBuilder fluent interface.
    /// </summary>
    public class QueryBuilderTests
    {
        [Fact]
        public void Build_WithNoPagination_ReturnsQueryWithNullPagination()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder.Build();

            // Assert
            result.Should().NotBeNull();
            result.Pagination.Should().BeNull();
        }

        [Fact]
        public void WithPagination_ValidValues_SetsPagination()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .WithPagination(2, 50)
                .Build();

            // Assert
            result.Pagination.Should().NotBeNull();
            result.Pagination.Page.Should().Be(2);
            result.Pagination.PageSize.Should().Be(50);
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(-5, 20)]
        public void WithPagination_NegativePage_ThrowsArgumentException(int page, int pageSize)
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.WithPagination(page, pageSize);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Page number must be non-negative*");
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, -1)]
        public void WithPagination_InvalidPageSize_ThrowsArgumentException(int page, int pageSize)
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.WithPagination(page, pageSize);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Page size must be positive*");
        }

        [Fact]
        public void SortBy_ValidField_AddsSortCriterion()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .SortBy("name", SortOrder.Asc)
                .Build();

            // Assert
            result.Sort.Should().NotBeNull();
            result.Sort.Should().HaveCount(1);
            result.Sort[0].Field.Should().Be("name");
            result.Sort[0].Order.Should().Be(SortOrder.Asc);
        }

        [Fact]
        public void SortBy_MultipleCriteria_AddsAllSorts()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .SortBy("name", SortOrder.Asc)
                .SortBy("createdAt", SortOrder.Desc)
                .Build();

            // Assert
            result.Sort.Should().NotBeNull();
            result.Sort.Should().HaveCount(2);
            result.Sort[0].Field.Should().Be("name");
            result.Sort[1].Field.Should().Be("createdAt");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SortBy_NullOrEmptyField_ThrowsArgumentException(string field)
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.SortBy(field);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Field name cannot be null or whitespace*");
        }

        [Fact]
        public void Where_ValidFilter_AddsFilter()
        {
            // Arrange
            var builder = new QueryBuilder();
            var filter = new EqualsOperator<string> { Field = "status", Value = "active" };

            // Act
            var result = builder
                .Where(filter)
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            result.Filters.And[0].Should().Be(filter);
        }

        [Fact]
        public void Where_NullFilter_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.Where((FilterOperator)null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhereAll_MultipleFilters_AddsAllFilters()
        {
            // Arrange
            var builder = new QueryBuilder();
            var filter1 = new EqualsOperator<string> { Field = "status", Value = "active" };
            var filter2 = new GreaterThanOperator<int> { Field = "age", Value = 18 };

            // Act
            var result = builder
                .WhereAll(filter1, filter2)
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            result.Filters.And[0].Should().BeOfType<AndOperator>();
            var andOp = (AndOperator)result.Filters.And[0];
            andOp.And.Should().HaveCount(2);
            andOp.And.Should().Contain(filter1);
            andOp.And.Should().Contain(filter2);
        }

        [Fact]
        public void WhereAll_NoFilters_DoesNotThrow()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder.WhereAll().Build();

            // Assert
            result.Filters.Should().BeNull();
        }

        [Fact]
        public void WithFacet_ValidField_AddsFacet()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .WithFacet("category", "categoryFacet", 20)
                .Build();

            // Assert
            result.Facets.Should().NotBeNull();
            result.Facets.Should().HaveCount(1);
            result.Facets[0].Field.Should().Be("category");
            result.Facets[0].Name.Should().Be("categoryFacet");
            result.Facets[0].Size.Should().Be(20);
        }

        [Fact]
        public void WithFacet_NoNameProvided_UsesFieldAsName()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .WithFacet("category")
                .Build();

            // Assert
            result.Facets[0].Name.Should().Be("category");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithFacet_NullOrEmptyField_ThrowsArgumentException(string field)
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.WithFacet(field);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Field name cannot be null or whitespace*");
        }

        [Fact]
        public void WithAliases_ValidAliases_AddsAliases()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .WithAliases("alias1", "alias2", "alias3")
                .Build();

            // Assert
            result.Aliases.Should().NotBeNull();
            result.Aliases.Should().HaveCount(3);
            result.Aliases.Should().Contain(new[] { "alias1", "alias2", "alias3" });
        }

        [Fact]
        public void WithAliases_NoAliases_DoesNotThrow()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder.WithAliases().Build();

            // Assert
            result.Aliases.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Build_CompleteQuery_ReturnsFullyConfiguredQuery()
        {
            // Arrange
            var builder = new QueryBuilder();
            var filter = new EqualsOperator<string> { Field = "status", Value = "active" };

            // Act
            var result = builder
                .WithPagination(0, 10)
                .SortBy("name", SortOrder.Asc)
                .Where(filter)
                .WithFacet("category", size: 15)
                .WithAliases("detailView")
                .Build();

            // Assert
            result.Should().NotBeNull();
            result.Pagination.Should().NotBeNull();
            result.Sort.Should().HaveCount(1);
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            result.Filters.And[0].Should().Be(filter);
            (result.Filters.And[0] is EqualsOperator<string>).Should().BeTrue();
            result.Facets.Should().HaveCount(1);
            result.Aliases.Should().HaveCount(1);
        }

        [Fact]
        public void Build_FluentChaining_AllowsMethodChaining()
        {
            // Arrange & Act
            var result = new QueryBuilder()
                .WithPagination(0, 5)
                .SortBy("updatedAt", SortOrder.Desc)
                .Where(new EqualsOperator<bool> { Field = "active", Value = true })
                .Build();

            // Assert
            result.Should().NotBeNull();
        }

        #region Lambda-Based Where() Tests

        [Fact]
        public void Where_WithLambdaFilter_BuildsFilter()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.Equals("status", "active"))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
        }

        [Fact]
        public void Where_WithLambdaAndMultipleConditions_WrapsInAndOperator()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f
                    .Equals("status", "active")
                    .GreaterThan("age", 18))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            result.Filters.And[0].Should().BeOfType<AndOperator>();
        }

        [Fact]
        public void Where_WithLambdaAndCaseInsensitive_SetsCaseInsensitiveProperty()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.Equals("status", "active", caseInsensitive: true))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            var equalsOp = result.Filters.And[0] as EqualsOperator<string>;
            equalsOp.Should().NotBeNull();
            equalsOp.CaseInsensitive.Should().BeTrue();
        }

        [Fact]
        public void Where_WithLambdaAndNestedOr_CreatesOrOperator()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f
                    .Equals("title", "hoodie")
                    .And().Or(o => o
                        .Equals("isInStock", true)
                        .Equals("allowPreorder", true)))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            
            var rootAnd = result.Filters.And[0] as AndOperator;
            rootAnd.Should().NotBeNull();
            rootAnd.And.Should().HaveCount(2);
            rootAnd.And[1].Should().BeOfType<OrOperator>();
        }

        [Fact]
        public void Where_MultipleLambdaCalls_AccumulatesWithAnd()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.Equals("status", "active"))
                .Where(f => f.GreaterThan("age", 18))
                .Where(f => f.NotEquals("role", "guest"))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(3);
        }

        [Fact]
        public void Where_MixedLambdaAndPowerUserAPI_AccumulatesAll()
        {
            // Arrange
            var builder = new QueryBuilder();
            var powerUserFilter = new EqualsOperator<bool> { Field = "verified", Value = true };

            // Act
            var result = builder
                .Where(f => f.Equals("status", "active"))
                .Where(powerUserFilter)
                .Where(f => f.GreaterThan("age", 18))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(3);
            result.Filters.And[1].Should().Be(powerUserFilter);
        }

        [Fact]
        public void Where_WithLambdaAndEmptyFilter_DoesNotAddFilter()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => { })
                .Build();

            // Assert
            result.Filters.Should().BeNull();
        }

        [Fact]
        public void Where_WithNullLambda_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.Where((Action<IFilterBuilder>)null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Where_WithLambdaAndInOperator_AddsInOperator()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.In("category", "electronics", "computers", "phones"))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            result.Filters.And[0].Should().BeOfType<InOperator<string>>();
        }

        [Fact]
        public void Where_WithLambdaAndContainsOperator_AddsContainsOperator()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.Contains("description", "*premium*", caseInsensitive: true))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            var containsOp = result.Filters.And[0] as ContainsOperator<string>;
            containsOp.Should().NotBeNull();
            containsOp.CaseInsensitive.Should().BeTrue();
        }

        [Fact]
        public void Where_ComplexNestedLambda_BuildsCorrectStructure()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f
                    .Equals("status", "active")
                    .And().Or(o => o
                        .Equals("isInStock", true)
                        .Equals("allowPreorder", true))
                    .And().And(a => a
                        .GreaterThanOrEquals("price", 10)
                        .LessThanOrEquals("price", 100)))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            
            var rootAnd = result.Filters.And[0] as AndOperator;
            rootAnd.Should().NotBeNull();
            rootAnd.And.Should().HaveCount(3);
            
            // Verify structure
            rootAnd.And[0].Should().BeOfType<EqualsOperator<string>>();
            rootAnd.And[1].Should().BeOfType<OrOperator>();
            rootAnd.And[2].Should().BeOfType<AndOperator>();
        }

        [Fact]
        public void Where_WithLambdaAndPagination_CombinesBoth()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .WithPagination(0, 10)
                .Where(f => f.Equals("status", "active"))
                .SortBy("name", SortOrder.Asc)
                .Build();

            // Assert
            result.Pagination.Should().NotBeNull();
            result.Pagination.PageSize.Should().Be(10);
            result.Filters.Should().NotBeNull();
            result.Sort.Should().HaveCount(1);
        }

        [Fact]
        public void Where_WithLambdaAndFacets_CombinesBoth()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.Equals("status", "active"))
                .WithFacet("category", size: 20)
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Facets.Should().HaveCount(1);
            result.Facets[0].Size.Should().Be(20);
        }

        [Fact]
        public void Where_WithLambdaAndAliases_CombinesBoth()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f.Equals("isFeatured", true))
                .WithAliases("tile", "detail")
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Aliases.Should().HaveCount(2);
        }

        [Fact]
        public void Where_MultipleLambdaCallsWithComplexFilters_BuildsCorrectStructure()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            var result = builder
                .Where(f => f
                    .Equals("status", "active")
                    .And().GreaterThan("age", 18))
                .Where(f => f.Or(o => o
                    .Equals("role", "admin")
                    .Equals("role", "moderator")))
                .Where(f => f.NotEquals("banned", true))
                .Build();

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(3);
        }

        [Fact]
        public void Where_WithLambdaAndAllOperators_BuildsCorrectly()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
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

            // Assert
            result.Filters.Should().NotBeNull();
            result.Filters.And.Should().HaveCount(1);
            
            var rootAnd = result.Filters.And[0] as AndOperator;
            rootAnd.Should().NotBeNull();
            rootAnd.And.Should().HaveCount(8);
        }

        #endregion
    }
}

