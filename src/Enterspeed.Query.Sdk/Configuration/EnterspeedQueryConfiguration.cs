namespace Enterspeed.Query.Sdk.Configuration
{
    public class EnterspeedQueryConfiguration
    {
        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        public string BaseUrl { get; set; } = "https://query.enterspeed.com";

        /// <summary>
        /// Gets or sets timeout in seconds. Default: 60 seconds.
        /// </summary>
        public int ConnectionTimeout { get; set; } = 60;

        /// <summary>
        /// Gets or sets the current version for the Query Endpoint.
        /// </summary>
        public string QueryVersion { get; set; } = "1";
    }
}