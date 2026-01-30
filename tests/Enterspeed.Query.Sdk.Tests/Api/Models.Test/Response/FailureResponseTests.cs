// namespace Enterspeed.Query.Sdk.Tests.Api.Models.Test.Response;
//
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Enterspeed.Query.Sdk.Api.Models.Response;
// using FluentAssertions;
// using static VerifyXunit.Verifier;
// using Xunit;
//
// public class ErrorResponseTests
// {
//     [Fact]
//     public Task Constructor_WithErrors_InitializesProperties()
//     {
//         var errors = new List<QueryError>
//         {
//             new ("index1", "Error 1"),
//             new ("index2", "Error 2")
//         };
//         var response = new ErrorResponse<string>(errors);
//         return Verify(response);
//     }
//
//     [Fact]
//     public Task Constructor_WithNullErrors_InitializesWithDefaultError()
//     {
//         var response = new ErrorResponse<string>((List<QueryError>)null);
//         return Verify(response);
//     }
//
//     [Fact]
//     public Task Constructor_WithEmptyErrors_AddsDefaultError()
//     {
//         var response = new ErrorResponse<string>(new List<QueryError>());
//         return Verify(response);
//     }
//
//     [Fact] // TODO DELETE THIS SINGLE WE DO NOT WANT ERROR RESPONSES WITH JUST STRINGS
//     public Task Constructor_WithErrorMessage_CreatesError()
//     {
//         var response = new ErrorResponse<QueryError>("Test error message");
//         return Verify(response);
//     }
//
//     [Fact]
//     public Task Constructor_WithSingleError_CreatesErrorList()
//     {
//         var error = new QueryError("Single error", "Name", "ERROR_CODE");
//         var response = new ErrorResponse<string>(error);
//         return Verify(response);
//     }
//
//     [Fact] // TODO DELETE THIS SINGLE WE DO NOT WANT ERROR RESPONSES WITH JUST STRINGS
//     public void Status_AlwaysReturnsFalse()
//     {
//         var response = new ErrorResponse<string>("Error");
//         response.Status.Should().BeFalse();
//     }
//
//     [Fact]
//     public void Errors_ReturnsReadOnlyList()
//     {
//         var errors = new List<QueryError>
//         {
//             new ("index1", "Test")
//         };
//         var response = new ErrorResponse<string>(errors);
//         var errorsList = response.Errors;
//         errorsList.Should().BeAssignableTo<IReadOnlyList<QueryError>>();
//     }
//
//     [Fact] // TODO DELETE THIS SINGLE WE DO NOT WANT ERROR RESPONSES WITH JUST STRINGS
//     public void ImplementsIFailure()
//     {
//         var response = new ErrorResponse<string>("Error");
//         response.Should().BeAssignableTo<IError<string>>();
//         response.Should().BeAssignableTo<IResponse>();
//     }
// }
