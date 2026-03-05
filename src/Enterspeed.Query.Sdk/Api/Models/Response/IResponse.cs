using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Base interface for all SDK responses.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Indicates whether the query was successful.
        /// </summary>
        bool Status { get; }
    }

    /// <summary>
    /// Base interface for typed SDK responses.
    /// </summary>
    /// <typeparam name="T">The expected type of the response data.</typeparam>
    public interface IResponse<T> : IResponse
    {
    }
}
