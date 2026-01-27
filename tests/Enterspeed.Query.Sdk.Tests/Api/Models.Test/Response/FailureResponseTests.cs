namespace Enterspeed.Query.Sdk.Tests.Api.Models.Test.Response;

using System.Collections.Generic;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models.Response;
using FluentAssertions;
using static VerifyXunit.Verifier;
using Xunit;

public class FailureResponseTests
{
    [Fact]
    public Task Constructor_WithErrors_InitializesProperties()
    {
        var errors = new List<QueryError>
        {
            new ("Error 1"),
            new ("Error 2")
        };
        var response = new FailureResponse(errors);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithNullErrors_InitializesWithDefaultError()
    {
        var response = new FailureResponse((List<QueryError>)null);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithEmptyErrors_AddsDefaultError()
    {
        var response = new FailureResponse(new List<QueryError>());
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithErrorMessage_CreatesError()
    {
        var response = new FailureResponse("Test error message");
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithSingleError_CreatesErrorList()
    {
        var error = new QueryError("Single error", "ERROR_CODE");
        var response = new FailureResponse(error);
        return Verify(response);
    }

    [Fact]
    public void Status_AlwaysReturnsFalse()
    {
        var response = new FailureResponse("Error");
        response.Status.Should().BeFalse();
    }

    [Fact]
    public void Errors_ReturnsReadOnlyList()
    {
        var errors = new List<QueryError>
        {
            new ("Test")
        };
        var response = new FailureResponse(errors);
        var errorsList = response.Errors;
        errorsList.Should().BeAssignableTo<IReadOnlyList<QueryError>>();
    }

    [Fact]
    public void ImplementsIFailure()
    {
        var response = new FailureResponse("Error");
        response.Should().BeAssignableTo<IFailure>();
        response.Should().BeAssignableTo<IResponse>();
    }
}

public class FailureResponseTypedTests
{
    private class TestData
    {
        public string Value { get; set; }
    }

    [Fact]
    public Task Constructor_WithErrors_InitializesProperties()
    {
        var errors = new List<QueryError>
        {
            new ("Error 1"),
            new ("Error 2")
        };
        var response = new FailureResponseTyped<TestData>(errors);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithNullErrors_InitializesWithDefaultError()
    {
        var response = new FailureResponseTyped<TestData>((List<QueryError>)null);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithEmptyErrors_AddsDefaultError()
    {
        var response = new FailureResponseTyped<TestData>(new List<QueryError>());
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithErrorMessage_CreatesError()
    {
        var response = new FailureResponseTyped<TestData>("Test error");
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithSingleError_CreatesErrorList()
    {
        var error = new QueryError("Single error", "ERROR_CODE");
        var response = new FailureResponseTyped<TestData>(error);
        return Verify(response);
    }

    [Fact]
    public void Status_AlwaysReturnsFalse()
    {
        var response = new FailureResponseTyped<TestData>("Error");
        response.Status.Should().BeFalse();
    }

    [Fact]
    public void Errors_ReturnsReadOnlyList()
    {
        var errors = new List<QueryError>
            {
                new ("Test")
            };
        var response = new FailureResponseTyped<TestData>(errors);
        var errorsList = response.Errors;
        errorsList.Should().BeAssignableTo<IReadOnlyList<QueryError>>();
    }

    [Fact]
    public void ImplementsIResponseOfT()
    {
        var response = new FailureResponseTyped<TestData>("Error");
        response.Should().BeAssignableTo<IResponse<TestData>>();
        response.Should().BeAssignableTo<IResponse>();
    }
}

public class QueryErrorTests
{
    [Fact]
    public Task Constructor_Default_InitializesEmptyDetails()
    {
        var error = new QueryError();
        return Verify(error);
    }

    [Fact]
    public Task Constructor_WithMessage_SetsMessage()
    {
        var error = new QueryError("Test message");
        return Verify(error);
    }

    [Fact]
    public Task Constructor_WithMessageAndCode_SetsBoth()
    {
        var error = new QueryError("Test message", "TEST_CODE");
        return Verify(error);
    }

    [Fact]
    public Task Details_CanBeModified()
    {
        var error = new QueryError("Test")
        {
            Details =
            {
                ["key"] = "value"
            }
        };
        return Verify(error);
    }
}
