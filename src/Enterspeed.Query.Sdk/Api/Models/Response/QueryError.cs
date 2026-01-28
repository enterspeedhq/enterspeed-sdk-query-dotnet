namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Represents an error that occurred during query execution.
    /// </summary>
    public class QueryError // TODO: Simplify since error is always the same way, so dont have multiple constructors and so on
    {
        // /// <summary>
        // /// Gets or sets the index
        // /// </summary>
        // public string Index { get; set; }
        public string Index { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; }

        // /// <summary>
        // /// Gets or sets the errors
        // /// </summary>
        // public string Errors { get; set; }
        public string[] Errors { get; set; }

        public QueryError()
        {
        }

        public QueryError(string index, string message, string[] errors = null)
        {
            Index = index;
            Message = message;
            Errors = errors;
        }
    }
}
