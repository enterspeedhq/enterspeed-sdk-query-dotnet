using System.Text.Json.Serialization;

namespace Example.Domain;

public record Persons
{
    public int Id { get; init; }

    public string Name { get; init; }

    public string Biography { get; init; }

    public DateTime BirthDate { get; init; }

    public string Gender { get; init; }

    [JsonPropertyName("place_of_birth")]
    public string PlaceOfBirth { get; init; }

    [JsonPropertyName("imdb_id")]
    public string ImdbId { get; init; }

    [JsonPropertyName("also_known_as")]
    public string[] AlsoKnownAs { get; init; }

    [JsonPropertyName("movies_ids")]
    public int MoviesIds { get; init; }
}
