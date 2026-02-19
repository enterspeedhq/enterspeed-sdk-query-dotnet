using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;
using Example.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Example;

public class QueriesController : Controller
{
    private readonly IEnterspeedQueryService _enterspeedQueryService;
    private readonly ILogger<QueriesController> _logger;
    public QueriesController(
        IEnterspeedQueryService enterspeedQueryService,
        ILogger<QueriesController> logger)
    {
        _logger = logger;
        _enterspeedQueryService = enterspeedQueryService;
    }

    [HttpGet("api/queries/single")]
    public async Task<IActionResult> GetQueryAsync()
    {
        var apiKey = "";

        var request = new MultiQueryBuilder()
            .AddQuery<Movie>("recent-high-budget-key", "movies", builder => builder
                .Where(f => f
                    .GreaterThanOrEquals(x => x.ReleaseDate, "2025-09-19")
                    .GreaterThan(x => x.Budget, 100000000))
                .SortBy(x => x.Popularity, SortOrder.Desc)
                .WithFacet(x => x.Genres)
                .WithPagination(0, 10))
            .Build();

        var response = await _enterspeedQueryService.Query(apiKey, request, CancellationToken.None);

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

    [HttpGet("api/queries/multi")]
    public async Task<IActionResult> GetQueriesAsync()
    {
        var apiKey = "";

        // Show how to build a multi-query request with 5 different queries using the MultiQueryBuilder
        var request = new MultiQueryBuilder()
            // 1. Recent High-Budget Movies
            .AddQuery<Movie>( "recent-high-budget-key", "movies", builder => builder
                .Where(f => f
                    .GreaterThanOrEquals(x => x.ReleaseDate, "2025-09-19")
                    .GreaterThan(x => x.Budget, 100000000))
                .SortBy(x => x.Popularity, SortOrder.Desc)
                .WithFacet(x => x.Genres)
                .WithPagination(0, 10))
            // 2. Top Rated Non-Adult Movies
            .AddQuery<Movie>("top-rated-non-adult-key", "movies", builder => builder
                .Where(f => f
                    .Equals(x => x.Adult, false)
                    .GreaterThan(x => x.VoteAverage, 8))
                .SortBy(x => x.VoteCount, SortOrder.Desc)
                .WithFacet(x => x.ProductionCompanies) // Is a string list field on the Query API
                .WithPagination(0, 5))
            // 3. Movies by Language and Runtime
            .AddQuery<Movie>("long-english-movies-key", "movies", builder => builder
                .Where(f => f
                    .Equals(x => x.OriginalLanguage, "en")
                    .GreaterThan(x => x.Runtime, 120))
                .SortBy(x => x.ReleaseDate, SortOrder.Asc)
                .WithFacet("productionCountries")
                .WithPagination(0, 15))
            // 4. Popular Movies with Specific Genre
            .AddQuery<Movie>("popular-action-movies-key", "movies", builder => builder
                .Where(f => f
                    .Contains(x => x.Genres, new string[] {"Action"})
                    .GreaterThan(x => x.Popularity, 50))
                .SortBy(x => x.VoteAverage, SortOrder.Desc)
                .WithFacet("spokenLanguages")
                .WithPagination(0, 8))
            // 5. Movies Released in a Date Range
            .AddQuery<Movie>( "movies-in-date-range-key", "movies", builder => builder
                .Where(f => f
                    .GreaterThanOrEquals(x => x.ReleaseDate, "2010-01-01")
                    .LessThanOrEquals("releaseDate", "2026-12-31"))
                .SortBy(x => x.Revenue, SortOrder.Desc)
                .WithFacet("genres")
                .WithPagination(0, 12))
            .Build();

        // Execute the multi-query request and get the response
        // Here we will only get some good request to show how we can handle the response both for errors and success
        var response = await _enterspeedQueryService.Query(apiKey, request, CancellationToken.None);

        //
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Multi-query request failed with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, "Failed to execute multi-query");
        }

        // Collect results for all queries
        var recentHighBudget = response.Get<MovieResponse>("recent-high-budget-key"); // There is already a Success<MovieResponse> or Failure<MovieResponse> in the response and we can handle them separately
        var topRatedNonAdult = response.Get<MovieResponse>("top-rated-non-adult-key"); // This is a IErrror<MovieResponse> since we have a failure for this query in the API response and we can handle it separately
        var longEnglishMovies = response.Get<MovieResponse>("long-english-movies-key");
        var popularActionMovies = response.Get<MovieResponse>("popular-action-movies-key");
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

        // var result = new
        // {
        //     RecentHighBudget = recentHighBudget is ISuccess<MovieResponse>,
        //     TopRatedNonAdult = topRatedNonAdult is ISuccess<MovieResponse>,
        //     LongEnglishMovies = longEnglishMovies is ISuccess<MovieResponse>,
        //     PopularActionMovies = popularActionMovies is ISuccess<MovieResponse>,
        //     MoviesInDateRange = moviesInDateRange is ISuccess<MovieResponse>
        // };

        if (recentHighBudget is ISuccess<MovieResponse>)
        {
            // Do some logic with the movies that works

        }









        // For demonstration we want to return the movies that works and not works




        return Ok(recentHighBudget);
    }
}
