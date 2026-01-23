using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Represents an error that occurred during query execution.
    /// </summary>
    public class QueryError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the error code, if applicable.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets additional details about the error.
        /// </summary>
        public Dictionary<string, object> Details { get; set; }

        public QueryError()
        {
            Details = new Dictionary<string, object>();
        }

        public QueryError(string message) : this()
        {
            Message = message;
        }

        public QueryError(string message, string code) : this(message)
        {
            Code = code;
        }
    }
}
