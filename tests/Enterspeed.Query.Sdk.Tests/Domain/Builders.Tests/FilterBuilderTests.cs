using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Tests.Domain.Builders.Tests;

using Enterspeed.Query.Sdk.Domain.Builders.Filter;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using static VerifyXunit.Verifier;
using Xunit;

public class FilterBuilderTests
{
    #region Comparison Operator Tests

    [Fact]
    public Task Equals_WithValidField_AddsEqualsOperator()
    {
        var builder = new FilterBuilder();
        builder.Equals("status", "active");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task Equals_WithCaseInsensitiveTrue_SetsCaseInsensitiveProperty()
    {
        var builder = new FilterBuilder();
        builder.Equals("status", "active", caseInsensitive: true);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task Equals_WithCaseInsensitiveFalse_SetsCaseInsensitiveProperty()
    {
        var builder = new FilterBuilder();
        builder.Equals("status", "active", caseInsensitive: false);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task Equals_WithNoCaseInsensitive_DoesNotSetProperty()
    {
        var builder = new FilterBuilder();
        builder.Equals("status", "active");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task NotEquals_WithValidField_AddsNotEqualsOperator()
    {
        var builder = new FilterBuilder();
        builder.NotEquals("role", "guest");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task NotEquals_WithCaseInsensitive_SetsCaseInsensitiveProperty()
    {
        var builder = new FilterBuilder();
        builder.NotEquals("role", "guest", caseInsensitive: true);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task GreaterThan_WithValidField_AddsGreaterThanOperator()
    {
        var builder = new FilterBuilder();
        builder.GreaterThan("age", 18);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task GreaterThanOrEquals_WithValidField_AddsGreaterThanOrEqualsOperator()
    {
        var builder = new FilterBuilder();
        builder.GreaterThanOrEquals("price", 100.50);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task LessThan_WithValidField_AddsLessThanOperator()
    {
        var builder = new FilterBuilder();
        builder.LessThan("stock", 10);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task LessThanOrEquals_WithValidField_AddsLessThanOrEqualsOperator()
    {
        var builder = new FilterBuilder();
        builder.LessThanOrEquals("discount", 50);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    #endregion

    #region String Operator Tests

    [Fact]
    public Task Contains_WithValidField_AddsContainsOperator()
    {
        var builder = new FilterBuilder();
        builder.Contains("description", "*premium*");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task Contains_WithCaseInsensitive_SetsCaseInsensitiveProperty()
    {
        var builder = new FilterBuilder();
        builder.Contains("title", "*hoodie*", caseInsensitive: true);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    #endregion

    #region Collection Operator Tests

    [Fact]
    public Task In_WithMultipleValues_AddsInOperator()
    {
        var builder = new FilterBuilder();
        builder.In("category", "electronics", "computers", "phones");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task In_WithSingleValue_AddsInOperator()
    {
        var builder = new FilterBuilder();
        builder.In("type", "premium");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    #endregion

    #region Filter Accumulation Tests

    [Fact]
    public Task MultipleFilters_AccumulatesAll()
    {
        var builder = new FilterBuilder();
        builder
            .Equals("status", "active")
            .GreaterThan("age", 18)
            .LessThan("age", 65);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task SingleFilter_ReturnsWithoutWrapping()
    {
        var builder = new FilterBuilder();
        builder.Equals("status", "active");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task NoFilters_ReturnsEmptyArray()
    {
        var builder = new FilterBuilder();
        var result = builder.BuildFilters();
        return Verify(result);
    }

    #endregion

    #region Logical Grouping Tests

    [Fact]
    public Task Or_WithLambda_CreatesOrOperator()
    {
        var builder = new FilterBuilder();
        builder.Or(o => o
            .Equals("status", "active")
            .Equals("status", "pending"));
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task Or_WithEmptyLambda_DoesNotAddOperator()
    {
        var builder = new FilterBuilder();
        builder.Or(_ => { });
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public void Or_WithNullLambda_ThrowsArgumentNullException()
    {
        var builder = new FilterBuilder();
        Action act = () => builder.Or(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public Task And_WithLambda_CreatesNestedAndOperator()
    {
        var builder = new FilterBuilder();
        builder.And(a => a
            .GreaterThan("price", 100)
            .LessThan("price", 200));
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task And_WithEmptyLambda_DoesNotAddOperator()
    {
        var builder = new FilterBuilder();
        builder.And(_ => { });
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public void And_WithNullLambda_ThrowsArgumentNullException()
    {
        var builder = new FilterBuilder();
        Action act = () => builder.And(null);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Complex Scenario Tests

    [Fact]
    public Task ComplexNestedFilters_BuildsCorrectStructure()
    {
        var builder = new FilterBuilder();
        builder
            .Equals("status", "active")
            .Or(o => o
                .Equals("isInStock", true)
                .Equals("allowPreorder", true))
            .And(a => a
                .GreaterThan("price", 10)
                .LessThan("price", 100));
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task MultipleOrGroups_BuildsCorrectStructure()
    {
        var builder = new FilterBuilder();
        builder
            .Or(o => o.Equals("type", "A").Equals("type", "B"))
            .Or(o => o.Equals("status", "active").Equals("status", "pending"));
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task MixedCaseInsensitiveAndRegularFilters_BuildsCorrectly()
    {
        var builder = new FilterBuilder();
        builder
            .Equals("status", "active", caseInsensitive: true)
            .Contains("title", "*hoodie*", caseInsensitive: true)
            .GreaterThan("price", 50);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    #endregion

    #region Typed Property Selector Tests

    private record Product
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Department { get; set; }
    }

    private record ProductWithJsonPropertyName
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("isActive")]
        public bool Active { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }
    }

    [Fact]
    public Task TypedEquals_WithPropertySelector_ExtractsFieldNameAndConvertsToCamelCase()
    {
        var builder = new FilterBuilder();
        builder.Equals<Product, bool>(p => p.Active, true);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedEquals_WithCaseInsensitive_SetsCaseInsensitiveProperty()
    {
        var builder = new FilterBuilder();
        builder.Equals<Product, string>(p => p.Name, "Hoodie", caseInsensitive: true);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedNotEquals_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.NotEquals<Product, string>(p => p.Department, "Electronics");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedGreaterThan_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.GreaterThan<Product, int>(p => p.Stock, 10);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedGreaterThanOrEquals_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.GreaterThanOrEquals<Product, decimal>(p => p.Price, 100.50m);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedLessThan_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.LessThan<Product, int>(p => p.Stock, 50);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedLessThanOrEquals_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.LessThanOrEquals<Product, decimal>(p => p.Price, 200m);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedContains_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.Contains<Product, string>(p => p.Name, "*shirt*");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedIn_WithPropertySelector_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.In<Product, string>(p => p.Department, "Clothing", "Shoes", "Accessories");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedIn_WithPropertySelectorIEnumerable_ExtractsFieldName()
    {
        var builder = new FilterBuilder();
        builder.In<Product, string>(p => p.Department, new List<string>
        {
            "Clothing",
            "Shoes",
            "Accessories"
        });
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedMultipleFilters_WithPropertySelectors_BuildsCorrectStructure()
    {
        var builder = new FilterBuilder();
        builder
            .Equals<Product, bool>(p => p.Active, true)
            .GreaterThan<Product, int>(p => p.Stock, 0)
            .LessThan<Product, decimal>(p => p.Price, 500m);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedAndString_MixedFilters_BuildsCorrectStructure()
    {
        var builder = new FilterBuilder();
        builder
            .Equals<Product, bool>(p => p.Active, true)
            .Equals("category", "electronics")
            .GreaterThan<Product, decimal>(p => p.Price, 50m);
        var result = builder.BuildFilters();
        return Verify(result);
    }

    [Fact]
    public Task TypedEquals_WithJsonPropertyNameAttribute_UsesJsonName()
    {
        var builder = new FilterBuilder<ProductWithJsonPropertyName>();
        builder
            .Equals(p => p.Active, true)
            .Equals(p => p.Stock, 12)
            .Equals(p => p.Name, "Gadget");
        var result = builder.BuildFilters();
        return Verify(result);
    }

    #endregion
}
