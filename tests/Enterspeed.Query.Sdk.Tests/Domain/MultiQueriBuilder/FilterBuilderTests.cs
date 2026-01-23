using System;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;
using Enterspeed.Query.Sdk.Domain.MultiQueriBuilder;
using FluentAssertions;
using Xunit;

namespace Enterspeed.Query.Sdk.Tests.Domain.MultiQueriBuilder
{
    /// <summary>
    /// Tests for the FilterBuilder internal implementation.
    /// </summary>
    public class FilterBuilderTests
    {
        #region Comparison Operator Tests

        [Fact]
        public void Equals_WithValidField_AddsEqualsOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Equals("status", "active");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<EqualsOperator<string>>();
            var op = (EqualsOperator<string>)result[0];
            op.Field.Should().Be("status");
            op.Value.Should().Be("active");
        }

        [Fact]
        public void Equals_WithCaseInsensitiveTrue_SetsCaseInsensitiveProperty()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Equals("status", "active", caseInsensitive: true);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            var op = (EqualsOperator<string>)result[0];
            op.CaseInsensitive.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithCaseInsensitiveFalse_SetsCaseInsensitiveProperty()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Equals("status", "active", caseInsensitive: false);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            var op = (EqualsOperator<string>)result[0];
            op.CaseInsensitive.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithNoCaseInsensitive_DoesNotSetProperty()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Equals("status", "active");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            var op = (EqualsOperator<string>)result[0];
            op.CaseInsensitive.Should().BeFalse(); // Default value
        }

        [Fact]
        public void NotEquals_WithValidField_AddsNotEqualsOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.NotEquals("role", "guest");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<NotEqualsOperator<string>>();
            var op = (NotEqualsOperator<string>)result[0];
            op.Field.Should().Be("role");
            op.Value.Should().Be("guest");
        }

        [Fact]
        public void NotEquals_WithCaseInsensitive_SetsCaseInsensitiveProperty()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.NotEquals("role", "guest", caseInsensitive: true);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            var op = (NotEqualsOperator<string>)result[0];
            op.CaseInsensitive.Should().BeTrue();
        }

        [Fact]
        public void GreaterThan_WithValidField_AddsGreaterThanOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.GreaterThan("age", 18);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<GreaterThanOperator<int>>();
            var op = (GreaterThanOperator<int>)result[0];
            op.Field.Should().Be("age");
            op.Value.Should().Be(18);
        }

        [Fact]
        public void GreaterThanOrEquals_WithValidField_AddsGreaterThanOrEqualsOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.GreaterThanOrEquals("price", 100.50);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<GreaterThanOrEqualsOperator<double>>();
            var op = (GreaterThanOrEqualsOperator<double>)result[0];
            op.Field.Should().Be("price");
            op.Value.Should().Be(100.50);
        }

        [Fact]
        public void LessThan_WithValidField_AddsLessThanOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.LessThan("stock", 10);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<LessThanOperator<int>>();
            var op = (LessThanOperator<int>)result[0];
            op.Field.Should().Be("stock");
            op.Value.Should().Be(10);
        }

        [Fact]
        public void LessThanOrEquals_WithValidField_AddsLessThanOrEqualsOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.LessThanOrEquals("discount", 50);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<LessThanOrEqualsOperator<int>>();
            var op = (LessThanOrEqualsOperator<int>)result[0];
            op.Field.Should().Be("discount");
            op.Value.Should().Be(50);
        }

        #endregion

        #region String Operator Tests

        [Fact]
        public void Contains_WithValidField_AddsContainsOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Contains("description", "*premium*");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<ContainsOperator<string>>();
            var op = (ContainsOperator<string>)result[0];
            op.Field.Should().Be("description");
            op.Value.Should().Be("*premium*");
        }

        [Fact]
        public void Contains_WithCaseInsensitive_SetsCaseInsensitiveProperty()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Contains("title", "*hoodie*", caseInsensitive: true);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            var op = (ContainsOperator<string>)result[0];
            op.CaseInsensitive.Should().BeTrue();
        }

        #endregion

        #region Collection Operator Tests

        [Fact]
        public void In_WithMultipleValues_AddsInOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.In("category", "electronics", "computers", "phones");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<InOperator<string>>();
            var op = (InOperator<string>)result[0];
            op.Field.Should().Be("category");
            op.Value.Should().BeEquivalentTo("electronics", "computers", "phones");
        }

        [Fact]
        public void In_WithSingleValue_AddsInOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.In("type", "premium");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<InOperator<string>>();
            var op = (InOperator<string>)result[0];
            op.Value.Should().HaveCount(1);
            op.Value.Should().Contain("premium");
        }

        #endregion

        #region Filter Accumulation Tests

        [Fact]
        public void MultipleFilters_AccumulatesAll()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder
                .Equals("status", "active")
                .GreaterThan("age", 18)
                .LessThan("age", 65);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<AndOperator>();
            var andOp = (AndOperator)result[0];
            andOp.And.Should().HaveCount(3);
        }

        [Fact]
        public void SingleFilter_ReturnsWithoutWrapping()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Equals("status", "active");
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<EqualsOperator<string>>();
        }

        [Fact]
        public void NoFilters_ReturnsEmptyArray()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            var result = builder.BuildFilters();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region Logical Grouping Tests

        [Fact]
        public void Or_WithLambda_CreatesOrOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Or(o => o
                .Equals("status", "active")
                .Equals("status", "pending"));
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<OrOperator>();
            var orOp = (OrOperator)result[0];
            orOp.Or.Should().HaveCount(1);
            orOp.Or[0].Should().BeOfType<AndOperator>();
        }

        [Fact]
        public void Or_WithEmptyLambda_DoesNotAddOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.Or(o => { });
            var result = builder.BuildFilters();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Or_WithNullLambda_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            Action act = () => builder.Or(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void And_WithLambda_CreatesNestedAndOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.And(a => a
                .GreaterThan("price", 100)
                .LessThan("price", 200));
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<AndOperator>();
            var andOp = (AndOperator)result[0];
            andOp.And.Should().HaveCount(1);
            andOp.And[0].Should().BeOfType<AndOperator>();
        }

        [Fact]
        public void And_WithEmptyLambda_DoesNotAddOperator()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder.And(a => { });
            var result = builder.BuildFilters();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void And_WithNullLambda_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            Action act = () => builder.And(null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region Complex Scenario Tests

        [Fact]
        public void ComplexNestedFilters_BuildsCorrectStructure()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder
                .Equals("status", "active")
                .Or(o => o
                    .Equals("isInStock", true)
                    .Equals("allowPreorder", true))
                .And(a => a
                    .GreaterThan("price", 10)
                    .LessThan("price", 100));
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<AndOperator>();
            var rootAnd = (AndOperator)result[0];
            rootAnd.And.Should().HaveCount(3);
            
            // First filter: Equals
            rootAnd.And[0].Should().BeOfType<EqualsOperator<string>>();
            
            // Second filter: OrOperator
            rootAnd.And[1].Should().BeOfType<OrOperator>();
            
            // Third filter: AndOperator (nested)
            rootAnd.And[2].Should().BeOfType<AndOperator>();
        }

        [Fact]
        public void MultipleOrGroups_BuildsCorrectStructure()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder
                .Or(o => o.Equals("type", "A").Equals("type", "B"))
                .Or(o => o.Equals("status", "active").Equals("status", "pending"));
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<AndOperator>();
            var andOp = (AndOperator)result[0];
            andOp.And.Should().HaveCount(2);
            andOp.And[0].Should().BeOfType<OrOperator>();
            andOp.And[1].Should().BeOfType<OrOperator>();
        }

        [Fact]
        public void MixedCaseInsensitiveAndRegularFilters_BuildsCorrectly()
        {
            // Arrange
            var builder = new FilterBuilder();

            // Act
            builder
                .Equals("status", "active", caseInsensitive: true)
                .Contains("title", "*hoodie*", caseInsensitive: true)
                .GreaterThan("price", 50);
            var result = builder.BuildFilters();

            // Assert
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<AndOperator>();
            var andOp = (AndOperator)result[0];
            andOp.And.Should().HaveCount(3);
            
            var equalsOp = (EqualsOperator<string>)andOp.And[0];
            equalsOp.CaseInsensitive.Should().BeTrue();
            
            var containsOp = (ContainsOperator<string>)andOp.And[1];
            containsOp.CaseInsensitive.Should().BeTrue();
            
            andOp.And[2].Should().BeOfType<GreaterThanOperator<int>>();
        }

        #endregion
    }
}
