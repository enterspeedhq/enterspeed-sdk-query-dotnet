namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Represents an error that occurred during query execution.
    /// </summary>
    public class QueryError
    {
        /// <summary>
        /// Gets or sets the index
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Gets the name used to identify the query.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the errors
        /// </summary>
        public string[] Errors { get; set; }

        public QueryError()
        {
        }

        public QueryError(string index, string name ,string message, string[] errors = null)
        {
            Index = index;
            Name = name;
            Message = message;
            Errors = errors;
        }
    }
}
