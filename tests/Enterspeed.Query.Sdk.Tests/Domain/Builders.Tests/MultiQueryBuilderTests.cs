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
        public void AddQuery_WithFluentBuilder_AddsQuerySuccessfully()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            var result = builder.AddQuery("users", "user-index", q => q
                .WithPagination(0, 10)
                .SortBy("name", SortOrder.Asc));

            // Assert
            result.Should().BeSameAs(builder, "builder should support fluent chaining");
            builder.Count.Should().Be(1);
            builder.ContainsKey("users").Should().BeTrue();
        }

        [Fact]
        public void AddQuery_WithPreConstructedQuery_AddsQuerySuccessfully()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            var query = new QueryObject
            {
                Pagination = new Pagination { Page = 0, PageSize = 5 }
            };

            // Act
            var result = builder.AddQuery("products", "product-index", query);

            // Assert
            result.Should().BeSameAs(builder);
            builder.Count.Should().Be(1);
            builder.ContainsKey("products").Should().BeTrue();
        }

        [Fact]
        public void AddQuery_MultipleQueries_AddsAllWithUniqueKeys()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder
                .AddQuery("users", "user-index", q => q.WithPagination(0, 10))
                .AddQuery("products", "product-index", q => q.WithPagination(0, 5))
                .AddQuery("orders", "order-index", new QueryObject());

            // Assert
            builder.Count.Should().Be(3);
            builder.ContainsKey("users").Should().BeTrue();
            builder.ContainsKey("products").Should().BeTrue();
            builder.ContainsKey("orders").Should().BeTrue();
        }

        [Fact]
        public void AddQuery_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("users", "user-index", q => q.WithPagination(0, 10));

            // Act
            Action act = () => builder.AddQuery("users", "another-index", q => q.WithPagination(0, 5));

            // Assert
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
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            Action act = () => builder.AddQuery(key, "index", q => { });

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Query key cannot be null or empty*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddQuery_NullOrEmptyIndex_ThrowsArgumentException(string index)
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            Action act = () => builder.AddQuery("key", index, q => { });

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Index cannot be null or empty*");
        }

        [Fact]
        public void AddQuery_NullBuilderAction_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            Action act = () => builder.AddQuery("key", "index", (Action<IQueryBuilder>)null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddQuery_NullQueryObject_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            Action act = () => builder.AddQuery("key", "index", (QueryObject)null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddQuery_ExceedsMaximumQueries_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("q1", "index", q => { });
            builder.AddQuery("q2", "index", q => { });
            builder.AddQuery("q3", "index", q => { });
            builder.AddQuery("q4", "index", q => { });
            builder.AddQuery("q5", "index", q => { });

            // Act
            Action act = () => builder.AddQuery("q6", "index", q => { });

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Cannot add more than 5 queries*");
        }

        #endregion

        #region Build Tests

        [Fact]
        public void Build_WithValidQueries_ReturnsImmutableRequest()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("users", "user-index", q => q.WithPagination(0, 10));

            // Act
            var request = builder.Build();

            // Assert
            request.Should().NotBeNull();
            request.Queries.Should().NotBeNull();
            request.Queries.Should().HaveCount(1);
        }

        [Fact]
        public void Build_NoQueries_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            Action act = () => builder.Build();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Cannot build a multi-query request with no queries*");
        }

        [Fact]
        public void Build_ReturnsReadOnlyCollection()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("users", "user-index", q => { });

            // Act
            var request = builder.Build();

            // Assert
            request.Queries.Should().BeAssignableTo<System.Collections.Generic.IReadOnlyList<MultiQueryObject>>();
        }

        [Fact]
        public void Build_AfterBuild_OriginalBuilderIsNotAffected()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("users", "user-index", q => { });
            var request1 = builder.Build();

            // Act
            builder.AddQuery("products", "product-index", q => { });
            var request2 = builder.Build();

            // Assert
            request1.Queries.Should().HaveCount(1, "first request should remain unchanged");
            request2.Queries.Should().HaveCount(2, "second request should have both queries");
        }

        #endregion

        #region Serialization & API Contract Tests

        [Fact]
        public void Build_QueryWithPagination_SerializesCorrectly()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("test", "test-index", q => q.WithPagination(2, 50));

            // Act
            var request = builder.Build();
            var query = request.Queries[0];

            // Assert
            query.Name.Should().Be("test");
            query.Index.Should().Be("test-index");
            query.Pagination.Should().NotBeNull();
            query.Pagination.Page.Should().Be(2);
            query.Pagination.PageSize.Should().Be(50);
        }

        [Fact]
        public void Build_QueryWithSorting_SerializesCorrectly()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("test", "test-index", q => q
                .SortBy("name", SortOrder.Asc)
                .SortBy("createdAt", SortOrder.Desc));

            // Act
            var request = builder.Build();
            var query = request.Queries[0];

            // Assert
            query.Sort.Should().NotBeNull();
            query.Sort.Should().HaveCount(2);
            query.Sort[0].Field.Should().Be("name");
            query.Sort[0].Order.Should().Be(SortOrder.Asc);
            query.Sort[1].Field.Should().Be("createdAt");
            query.Sort[1].Order.Should().Be(SortOrder.Desc);
        }

        [Fact]
        public void Build_QueryWithFilters_SerializesCorrectly()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            var filter = new EqualsOperator<string> { Field = "status", Value = "active" };

            builder.AddQuery("test", "test-index", q => q.Where(filter));

            // Act
            var request = builder.Build();
            var query = request.Queries[0];

            // Assert
            query.Filters.Should().NotBeNull();
            query.Filters.And.Should().HaveCount(1);
            query.Filters.And[0].Should().Be(filter);
        }

        [Fact]
        public void Build_QueryWithFacets_SerializesCorrectly()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("test", "test-index", q => q
                .WithFacet("category", "categoryFacet", 25));

            // Act
            var request = builder.Build();
            var query = request.Queries[0];

            // Assert
            query.Facets.Should().NotBeNull();
            query.Facets.Should().HaveCount(1);
            query.Facets[0].Field.Should().Be("category");
            query.Facets[0].Name.Should().Be("categoryFacet");
            query.Facets[0].Size.Should().Be(25);
        }

        [Fact]
        public void Build_QueryWithAliases_SerializesCorrectly()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("test", "test-index", q => q
                .WithAliases("view1", "view2"));

            // Act
            var request = builder.Build();
            var query = request.Queries[0];

            // Assert
            query.Aliases.Should().NotBeNull();
            query.Aliases.Should().HaveCount(2);
            query.Aliases.Should().Contain(new[] { "view1", "view2" });
        }

        [Fact]
        public void Build_ComplexMultiQuery_MatchesAPIContract()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            builder
                .AddQuery("users", "user-index", q => q
                    .WithPagination(0, 10)
                    .SortBy("updatedAt", SortOrder.Desc)
                    .Where(new EqualsOperator<bool> { Field = "active", Value = true })
                    .WithFacet("department"))
                .AddQuery("products", "product-index", q => q
                    .WithPagination(0, 20)
                    .SortBy("price", SortOrder.Asc)
                    .WithAliases("tile", "detail"));

            // Act
            var request = builder.Build();

            // Assert
            request.Queries.Should().HaveCount(2);

            var userQuery = request.Queries[0];
            userQuery.Name.Should().Be("users");
            userQuery.Index.Should().Be("user-index");
            userQuery.Pagination.PageSize.Should().Be(10);
            userQuery.Filters.Should().NotBeNull();

            var productQuery = request.Queries[1];
            productQuery.Name.Should().Be("products");
            productQuery.Index.Should().Be("product-index");
            productQuery.Aliases.Should().HaveCount(2);
        }

        #endregion

        #region Helper Method Tests

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("users", "user-index", q => { });

            // Act
            var result = builder.ContainsKey("users");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var builder = new MultiQueryBuilder();
            builder.AddQuery("users", "user-index", q => { });

            // Act
            var result = builder.ContainsKey("products");

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ContainsKey_NullOrEmptyKey_ReturnsFalse(string key)
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            var result = builder.ContainsKey(key);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Count_InitiallyZero()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act & Assert
            builder.Count.Should().Be(0);
        }

        [Fact]
        public void Count_IncreasesWithEachQuery()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery("q1", "index", q => { });
            builder.AddQuery("q2", "index", q => { });
            builder.AddQuery("q3", "index", q => { });

            // Assert
            builder.Count.Should().Be(3);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void FluentAPI_CompleteScenario_BuildsCorrectRequest()
        {
            // Arrange & Act
            var request = new MultiQueryBuilder()
                .AddQuery("activeUsers", "user-index", builder => builder
                    .WithPagination(0, 25)
                    .SortBy("lastName", SortOrder.Asc)
                    .SortBy("firstName", SortOrder.Asc)
                    .Where(new EqualsOperator<bool> { Field = "isActive", Value = true })
                    .WithFacet("role", size: 10)
                    .WithAliases("summary"))
                .AddQuery("recentOrders", "order-index", builder => builder
                    .WithPagination(0, 50)
                    .SortBy("orderDate", SortOrder.Desc)
                    .WhereAll(
                        new GreaterThanOperator<string> { Field = "orderDate", Value = "2025-01-01" }, // TODO: Should it be DateOnly or string?
                        new EqualsOperator<string> { Field = "status", Value = "completed" }))
                .AddQuery("popularProducts", "product-index", new QueryObject
                {
                    Pagination = new Pagination { Page = 0, PageSize = 10 },
                    Sort = new System.Collections.Generic.List<Sort>
                    {
                        new Sort { Field = "viewCount", Order = SortOrder.Desc }
                    }
                })
                .Build();

            // Assert
            request.Should().NotBeNull();
            request.Queries.Should().HaveCount(3);

            // Verify first query
            var activeUsersQuery = request.Queries[0];
            activeUsersQuery.Name.Should().Be("activeUsers");
            activeUsersQuery.Index.Should().Be("user-index");
            activeUsersQuery.Pagination.PageSize.Should().Be(25);
            activeUsersQuery.Sort.Should().HaveCount(2);
            activeUsersQuery.Filters.And.Should().HaveCount(1);
            activeUsersQuery.Facets.Should().HaveCount(1);
            activeUsersQuery.Aliases.Should().HaveCount(1);

            // Verify second query
            var recentOrdersQuery = request.Queries[1];
            recentOrdersQuery.Name.Should().Be("recentOrders");
            recentOrdersQuery.Filters.And.Should().HaveCount(1);
            recentOrdersQuery.Filters.And[0].Should().BeOfType<AndOperator>();
            var andOp = (AndOperator)recentOrdersQuery.Filters.And[0];
            andOp.And.Should().HaveCount(2);

            // Verify third query (pre-constructed)
            var popularProductsQuery = request.Queries[2];
            popularProductsQuery.Name.Should().Be("popularProducts");
            popularProductsQuery.Pagination.PageSize.Should().Be(10);
        }

        #endregion
    }
}
