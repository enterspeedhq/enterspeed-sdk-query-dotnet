using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Non-generic interface for failed responses, enabling pattern matching without knowing T.
    /// </summary>
    public interface IError : IResponse
    {
        /// <summary>
        /// Gets the collection of errors that caused the query to fail.
        /// </summary>
        IReadOnlyList<QueryError> Errors { get; }
    }

    /// <summary>
    /// Represents a failed query response with error information.
    /// </summary>
    public interface IError<T> : IError, IResponse<T>
    {
        // Inherits Errors from IError and adds IResponse<T> support
    }
}
