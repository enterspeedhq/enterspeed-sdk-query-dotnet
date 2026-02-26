using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;
using Example.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers;

public class QueriesFluentController : Controller
{
    private readonly IEnterspeedQueryService _enterspeedQueryService;
    private readonly ILogger<QueriesTypedController> _logger;
    private const string ApiKey = "";
    public QueriesFluentController(
        IEnterspeedQueryService enterspeedQueryService,
        ILogger<QueriesTypedController> logger)
    {
        _logger = logger;
        _enterspeedQueryService = enterspeedQueryService;
    }

    [HttpGet("api/fluent/single")]
    public async Task<IActionResult> GetQueryAsync()
    {
        var request = new QueryBuilder()
            .AddQuery("recent-high-budget-key", "movies", builder => builder
                .Where(f => f
                    .GreaterThanOrEquals("release_date", DateTimeOffset.Parse("2025-09-19"))
                    .GreaterThan("budget", 100000000))
                .SortBy("popularity", SortOrder.Desc)
                .WithFacet("genres")
                .WithPagination(0, 10))
            .Build();

        var response = await _enterspeedQueryService.Query(ApiKey, request, CancellationToken.None);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Multi-query request failed with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, "Failed to execute multi-query");
        }

        var recentHighBudget = response.Get<MovieResponse>("recent-high-budget-key");

        return recentHighBudget switch
        {
            IError recentHighBudgetError => BadRequest(recentHighBudgetError),
            ISuccess<MovieResponse> recentHighBudgetSuccess => Ok(recentHighBudgetSuccess),
            _ => Ok(recentHighBudget) // The MovieResponse is already a success and can be return, however note to use it has to be ISuccess<MovieResponse>
        };
    }

    [HttpGet("api/fluent/multi")]
    public async Task<IActionResult> GetQueriesAsync()
    {
        // Show how to build a multi-query request with 5 different queries using the MultiQueryBuilder
        var request = new QueryBuilder()
            .AddQuery("recent-high-budget-key", "movies", builder => builder
                .Where(f => f
                    .GreaterThanOrEquals("release_date", DateTimeOffset.Parse("2025-09-19"))
                    .GreaterThan("budget", 100000000))
                .SortBy("popularity", SortOrder.Desc)
                .WithFacet("genres")
                .WithPagination(0, 10))
            .AddQuery("top-rated-non-adult-key", "movies", builder => builder
                .Where(f => f
                    .Equals("adult", false)
                    .GreaterThan("vote_average", 8))
                .SortBy("vote_count", SortOrder.Desc)
                .WithFacet("production_companies") // Is a string list field on the Query API
                .WithPagination(0, 5))
            .AddQuery<MovieCredits>("movie-credits", "movieCredits", builder => builder
                .WithFacet("productionCountries")
                .WithPagination(0, 15))
            .AddQuery<Persons>("persons", "persons", builder => builder
                .Where(f => f
                    .Equals("gender", "Female")
                    .GreaterThan("birthday", new DateTime(1980, 1, 1)))
                .SortBy("name", SortOrder.Desc)
                .WithPagination(0, 8))
            .AddQuery("movies-in-date-range-key", "movies", builder => builder
                .Where(f => f
                    .GreaterThanOrEquals("release_date", DateTimeOffset.Parse("2010-01-01"))
                    .LessThanOrEquals("release_date", DateTimeOffset.Parse("2026-12-31")))
                .SortBy("revenue", SortOrder.Desc)
                .WithFacet("genres")
                .WithPagination(0, 12))
            .Build();

        // Execute the multi-query request and get the response
        // Here we will only get some good request to show how we can handle the response both for errors and success
        var response = await _enterspeedQueryService.Query(ApiKey, request, CancellationToken.None);

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Multi-query request failed with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, response.Message);
        }

        // Collect results for all queries
        var recentHighBudget = response.Get<MovieResponse>("recent-high-budget-key");
        var topRatedNonAdult = response.Get<MovieResponse>("top-rated-non-adult-key");
        var movieCredits = response.Get<MovieCredits>("movie-credits");
        var persons = response.Get<Persons>("persons");
        var moviesInDateRange = response.Get<MovieResponse>("movies-in-date-range-key");

        if (recentHighBudget is IError recentHighBudgetError)
        {
            foreach (var queryError in recentHighBudgetError.Errors)
            {
                _logger.LogInformation($"Error for 'recent-high-budget-key': {queryError.Errors}");
            }
        }

        // We then want base on the Movies to do some logic to get the movies that does not work
        if (topRatedNonAdult is IError topRatedNonAdultFailure)
        {
            foreach (var queryError in topRatedNonAdultFailure.Errors)
            {
                _logger.LogInformation($"Error for 'top-rated-non-adult-key': {queryError.Errors}");
            }
        }

        if (topRatedNonAdult is ISuccess<MovieResponse> topRatedNonAdultSuccess)
        {
            var inception = topRatedNonAdultSuccess.Results.FirstOrDefault(x => x.OriginalTitle == "Inception");

            if (inception != null)
            {
                _logger.LogInformation($"Inception found with vote average: {inception.VoteAverage}");
            }

            var englishMoviesOfTopRatedNonAdult = topRatedNonAdultSuccess.Results.Where(x => x.OriginalLanguage == "en").ToList();
            _logger.LogInformation("English movies of top-rated non-adult movies:");
            englishMoviesOfTopRatedNonAdult.ForEach(movie => _logger.LogInformation("English movie: {MovieOriginalTitle} with vote average: {MovieVoteAverage}", movie.OriginalTitle, movie.VoteAverage));

            // Handle MovieCredits
            if (movieCredits is ISuccess<MovieCredits> movieCreditsSuccess)
            {
                foreach (var credits in movieCreditsSuccess.Results)
                {
                    _logger.LogInformation($"MovieCredits found. MovieId: {credits.MovieId}");
                    var castType = credits.Cast.GetType().Name;
                    _logger.LogInformation($"Cast type: {castType}");
                    if (credits.Cast is IEnumerable<object> castList)
                    {
                        _logger.LogInformation($"Cast count: {castList.Count()}");
                    }

                }
            }

            // Handle Persons
            if (persons is ISuccess<Persons> personsSuccess)
            {

                foreach (var person in personsSuccess.Results)
                {
                    _logger.LogInformation($"Person found: {person.Name}, BirthDate: {person.BirthDate:yyyy-MM-dd}, Gender: {person.Gender}");
                    if (person.AlsoKnownAs.Length > 0)
                    {
                        _logger.LogInformation($"Also known as: {string.Join(", ", person.AlsoKnownAs)}");
                    }
                }
            }
        }

        // Relay the response where derived types are used. Same response as the Query API
        return Ok(response.Response);
    }
}
