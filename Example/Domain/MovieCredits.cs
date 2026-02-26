using System.Text.Json.Serialization;

namespace Example.Domain;

public record MovieCredits
{
    [JsonPropertyName("movie_id")]
    public int MovieId { get; init; }

    public object Cast { get; init; }
}
