using System.Collections.Generic;
using System.Linq;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Implementation of IFailure for failed query responses.
    /// Exposes error information and does not expose typed data or Value() method.
    /// </summary>
    public class FailureResponse : IFailure
    {
        private readonly List<QueryError> _errors;

        public FailureResponse(List<QueryError> errors)
        {
            _errors = errors ?? new List<QueryError>();
            
            // Ensure at least one error exists
            if (_errors.Count == 0)
            {
                _errors.Add(new QueryError("An unknown error occurred"));
            }
        }

        public FailureResponse(string errorMessage) : this(new List<QueryError>())
        {
            _errors.Clear();
            _errors.Add(new QueryError(errorMessage));
        }

        public FailureResponse(QueryError error) : this(new List<QueryError> { error })
        {
        }

        /// <inheritdoc />
        public bool Status => false;

        /// <inheritdoc />
        public IReadOnlyList<QueryError> Errors => _errors.AsReadOnly();
    }

    /// <summary>
    /// Generic implementation of IFailure for typed query responses.
    /// Implements IResponse&lt;T&gt; but only exposes error information.
    /// </summary>
    /// <typeparam name="T">The expected type (not used, as this is a failure).</typeparam>
    public class FailureResponseTyped<T> : IResponse<T>
    {
        private readonly List<QueryError> _errors;

        public FailureResponseTyped(List<QueryError> errors)
        {
            _errors = errors ?? new List<QueryError>();
            
            // Ensure at least one error exists
            if (_errors.Count == 0)
            {
                _errors.Add(new QueryError("An unknown error occurred"));
            }
        }

        public FailureResponseTyped(string errorMessage) : this(new List<QueryError>())
        {
            _errors.Clear();
            _errors.Add(new QueryError(errorMessage));
        }

        public FailureResponseTyped(QueryError error) : this(new List<QueryError> { error })
        {
        }

        /// <inheritdoc />
        public bool Status => false;

        /// <summary>
        /// Gets the collection of errors. Exposed for pattern matching.
        /// </summary>
        public IReadOnlyList<QueryError> Errors => _errors.AsReadOnly();
    }
}
