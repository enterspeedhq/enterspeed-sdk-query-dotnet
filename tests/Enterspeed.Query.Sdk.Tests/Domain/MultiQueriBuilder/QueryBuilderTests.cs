using System;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
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
                .WithMessage("*Page must be non-negative*");
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
                .WithMessage("*Page size must be at least 1*");
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
                .WithMessage("*Field cannot be null or empty*");
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
            Action act = () => builder.Where(null);

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
            result.Filters.And.Should().HaveCount(2);
            result.Filters.And.Should().Contain(filter1);
            result.Filters.And.Should().Contain(filter2);
        }

        [Fact]
        public void WhereAll_NoFilters_ThrowsArgumentException()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.WhereAll();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*At least one filter must be provided*");
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
                .WithMessage("*Field cannot be null or empty*");
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
        public void WithAliases_NoAliases_ThrowsArgumentException()
        {
            // Arrange
            var builder = new QueryBuilder();

            // Act
            Action act = () => builder.WithAliases();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*At least one alias must be provided*");
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
    }
}
