using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Represents a failed query response with error information.
    /// </summary>
    public interface IError<T> : IResponse<T>
    {
        /// <summary>
        /// Gets the collection of errors that caused the query to fail.
        /// </summary>
        IReadOnlyList<QueryError> Errors { get; }
    }
}
