namespace Enterspeed.Query.Sdk.Tests.Domain.Builders.Tests
{
    using System;
    using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
    using Enterspeed.Query.Sdk.Domain.Models;
    using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
    using Xunit;

    /// <summary>
    /// Tests for the typed query builder functionality where entity type is inferred from AddQuery&lt;T&gt;.
    /// Verifies that the syntax .Where(x => x.Equals(p => p.Property, value)) works as expected.
    /// </summary>
    public class TypedQueryBuilderTests
    {
        private class Product
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public bool Active { get; set; }
            public string Department { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        [Fact]
        public void AddQuery_WithTypedFilters_BuildsCorrectQuery()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .WithPagination(0, 20)
                .SortBy(p => p.UpdatedAt, SortOrder.Desc)
                .Where(filter => filter
                    .Equals(p => p.Active, true)));

            var request = builder.Build();

            // Assert
            Assert.NotNull(request);
            Assert.Single(request.Queries);

            var query = request.Queries[0];
            Assert.Equal("products", query.Name);
            Assert.Equal("product-index", query.Index);

            // Verify pagination
            Assert.NotNull(query.Pagination);
            Assert.Equal(0, query.Pagination.Page);
            Assert.Equal(20, query.Pagination.PageSize);

            // Verify sort
            Assert.NotNull(query.Sort);
            Assert.Single(query.Sort);
            Assert.Equal("updatedAt", query.Sort[0].Field); // Should be camelCase
            Assert.Equal(SortOrder.Desc, query.Sort[0].Order);

            // Verify filters
            Assert.NotNull(query.Filters);
            // Assert.Single(query.Filters); // TODO Figure out this
        }

        [Fact]
        public void AddQuery_WithMultipleTypedFilters_BuildsCorrectQuery()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .Where(filter => filter
                    .Equals(p => p.Active, true)
                    .GreaterThan(p => p.Price, 50m)
                    .In(p => p.Department, "Electronics", "Computers")));

            var request = builder.Build();

            // Assert
            Assert.NotNull(request);
            var query = request.Queries[0];
            Assert.NotNull(query.Filters);
            Assert.Equal(3, query.Filters.And.Count);
        }

        [Fact]
        public void AddQuery_WithTypedSortBy_UsesCorrectFieldName()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .SortBy(p => p.Price, SortOrder.Asc));

            var request = builder.Build();

            // Assert
            var query = request.Queries[0];
            Assert.NotNull(query.Sort);
            Assert.Single(query.Sort);
            Assert.Equal("price", query.Sort[0].Field); // Should be camelCase
            Assert.Equal(SortOrder.Asc, query.Sort[0].Order);
        }

        [Fact]
        public void AddQuery_WithTypedFacet_UsesCorrectFieldName()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .WithFacet(p => p.Department, "departmentFacet", 10));

            var request = builder.Build();

            // Assert
            var query = request.Queries[0];
            Assert.NotNull(query.Facets);
            Assert.Single(query.Facets);
            Assert.Equal("department", query.Facets[0].Field); // Should be camelCase
            Assert.Equal("departmentFacet", query.Facets[0].Name);
            Assert.Equal(10, query.Facets[0].Size);
        }

        [Fact]
        public void AddQuery_WithNestedOrOperator_BuildsCorrectQuery()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .Where(filter => filter
                    .Equals(p => p.Active, true)
                    .Or(orFilter => orFilter
                        .Equals(p => p.Department, "Electronics")
                        .Equals(p => p.Department, "Computers"))));

            var request = builder.Build();

            // Assert
            Assert.NotNull(request);
            var query = request.Queries[0];
            Assert.NotNull(query.Filters);
            Assert.Equal(2, query.Filters.And.Count); // Active filter + OR group
        }

        [Fact]
        public void AddQuery_WithMultipleDifferentTypes_EachHasCorrectType()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .Where(filter => filter
                    .Equals(p => p.Active, true)));  // p is Product

            builder.AddQuery<Product>("other-products", "product-index", q => q
                .Where(filter => filter
                    .GreaterThan(p => p.Price, 100m)));  // p is also Product

            var request = builder.Build();

            // Assert
            Assert.Equal(2, request.Queries.Count);
        }

        [Fact]
        public void AddQuery_MixingTypedAndStringMethods_WorksTogether()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .SortBy(p => p.Price, SortOrder.Desc)
                .SortBy(p => p.Name, SortOrder.Asc)
                .Where(filter => filter
                    .Equals(p => p.Active, true)          // Typed
                    .GreaterThan("customField", 100)));   // TODO: Should this be removed String-based

            var request = builder.Build();

            // Assert
            var query = request.Queries[0];
            Assert.Equal(2, query.Sort.Count);
            Assert.Equal(2, query.Filters.And.Count);
        }

        [Fact]
        public void AddQuery_WithCaseInsensitiveFilter_PreservesFlag()
        {
            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .Where(filter => filter
                    .Equals(p => p.Name, "laptop", caseInsensitive: true)));

            var request = builder.Build();

            // Assert
            var query = request.Queries[0];
            Assert.NotNull(query.Filters);
            var equalsOperator = query.Filters.And[0] as EqualsOperator<string>;
            Assert.NotNull(equalsOperator);
            Assert.True(equalsOperator.CaseInsensitive);
        }

        [Fact]
        public void TypedQueryBuilder_PropertyNameConversion_UsesCamelCase()
        {
            // This test verifies that property names are converted from PascalCase to camelCase
            // UpdatedAt -> updatedAt, Active -> active, etc.

            // Arrange
            var builder = new MultiQueryBuilder();

            // Act
            builder.AddQuery<Product>("products", "product-index", q => q
                .SortBy(p => p.UpdatedAt, SortOrder.Desc)
                .Where(filter => filter
                    .Equals(p => p.Active, true))
                .WithFacet(p => p.Department));

            var request = builder.Build();

            // Assert
            var query = request.Queries[0];

            // Sort field should be camelCase
            Assert.Equal("updatedAt", query.Sort[0].Field);

            // Facet field should be camelCase
            Assert.Equal("department", query.Facets[0].Field);
        }
    }
}
