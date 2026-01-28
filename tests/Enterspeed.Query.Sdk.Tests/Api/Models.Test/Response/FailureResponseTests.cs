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
            new ("index1", "Error 1"),
            new ("index2", "Error 2")
        };
        var response = new FailureResponse<string>(errors);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithNullErrors_InitializesWithDefaultError()
    {
        var response = new FailureResponse<string>((List<QueryError>)null);
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithEmptyErrors_AddsDefaultError()
    {
        var response = new FailureResponse<string>(new List<QueryError>());
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithErrorMessage_CreatesError()
    {
        var response = new FailureResponse<string>("Test error message");
        return Verify(response);
    }

    [Fact]
    public Task Constructor_WithSingleError_CreatesErrorList()
    {
        var error = new QueryError("Single error", "ERROR_CODE");
        var response = new FailureResponse<string>(error);
        return Verify(response);
    }

    [Fact]
    public void Status_AlwaysReturnsFalse()
    {
        var response = new FailureResponse<string>("Error");
        response.Status.Should().BeFalse();
    }

    [Fact]
    public void Errors_ReturnsReadOnlyList()
    {
        var errors = new List<QueryError>
        {
            new ("index1", "Test")
        };
        var response = new FailureResponse<string>(errors);
        var errorsList = response.Errors;
        errorsList.Should().BeAssignableTo<IReadOnlyList<QueryError>>();
    }

    [Fact]
    public void ImplementsIFailure()
    {
        var response = new FailureResponse<string>("Error");
        response.Should().BeAssignableTo<IFailure<string>>(); // TODO: Check if it should be IFailure for all failures or do we want other behaviour but what will when happen to polymorphic casting?
        response.Should().BeAssignableTo<IResponse>();
    }
}
