using System.Text.Json.Serialization;

namespace Example.Domain;

public record Movie
{
    public bool Adult { get; set; }
    public object Images { get; set; }
    public int Budget { get; set; }
    public string[] Genres { get; set; }
    public string Id { get; set; }

    [JsonPropertyName("imdb_id")]
    public string ImdbId { get; set; }

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; }

    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; set; }
    public string Overview { get; set; }
    public double Popularity { get; set; }

    [JsonPropertyName("production_companies")]
    public string[] ProductionCompanies { get; set; }

    [JsonPropertyName("production_countries")]
    public string[] ProductionCountries { get; set; }

    [JsonPropertyName("release_date")]
    public DateTimeOffset ReleaseDate { get; set; }
    public int Revenue { get; set; }
    public int Runtime { get; set; }

    [JsonPropertyName("spoken_languages")]
    public string[] SpokenLanguages { get; set; }
    public string Title { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }
}
