using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Represents a failed response for SDK consumers.
    /// </summary>
    /// <typeparam name="T">The expected type (not used, as this is a failure).</typeparam>
    public class ErrorResponse<T> : IError<T>
    {
        private readonly List<QueryError> _errors;

        public ErrorResponse(List<QueryError> errors)
        {
            _errors = errors ?? new List<QueryError>();
            if (_errors.Count == 0)
            {
                _errors.Add(new QueryError("No Index", "No query to identify", "An unknown error occurred"));
            }
        }

        public ErrorResponse(QueryError error) : this(new List<QueryError> { error })
        {
        }

        public bool Status => false;

        public IReadOnlyList<QueryError> Errors => _errors.AsReadOnly();
    }
}
