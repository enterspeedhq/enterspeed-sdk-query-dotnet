using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;
using Example.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers;

public class DirectQueryObjectController : Controller
{
    private readonly IEnterspeedQueryService _enterspeedQueryService;
    private readonly ILogger<QueriesTypedController> _logger;
    private const string ApiKey = "";
    public DirectQueryObjectController(
        IEnterspeedQueryService enterspeedQueryService,
        ILogger<QueriesTypedController> logger)
    {
        _logger = logger;
        _enterspeedQueryService = enterspeedQueryService;
    }

    [HttpGet("api/directqueryobject/single")]
    public async Task<IActionResult> GetQueryAsync()
    {
        var request = new QueryBuilder()
            .AddQuery("recent-high-budget-key", "movies", new QueryObject
            {
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new GreaterThanOrEqualsOperator<string>
                        {
                            Field = "release_date",
                            Value = DateTimeOffset.Parse("2025-09-19").ToString()
                        },
                        new GreaterThanOperator<int>
                        {
                            Field = "budget",
                            Value = 100000000
                        }
                    }
                },
                Sort = new List<Sort>
                {
                    new Sort
                    {
                        Field = "popularity",
                        Order = SortOrder.Desc
                    }
                },
                Facets = new List<Facet>
                {
                    new Facet
                    {
                        Field = "genres",
                        Name = "Genres"
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            })
            .Build();

        var response = await _enterspeedQueryService.Query(ApiKey, request, CancellationToken.None);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            Console.WriteLine($"Multi-query request failed with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, "Insufficient scope for accessing indices for current api-key");
        }

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"Multi-query request failed with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, response.Message);
        }

        var recentHighBudget = response.Get<MovieResponse>("recent-high-budget-key");

        return recentHighBudget switch
        {
            IError recentHighBudgetError => BadRequest(recentHighBudgetError),
            ISuccess<MovieResponse> recentHighBudgetSuccess => Ok(recentHighBudgetSuccess),
            _ => Ok(recentHighBudget) // The MovieResponse is already a success and can be return, however note to use it has to be ISuccess<MovieResponse>
        };
    }

    [HttpGet("api/directqueryobject/multi")]
    public async Task<IActionResult> GetQueriesAsync()
    {
        // Show how to build a multi-query request with 5 different queries using the MultiQueryBuilder
        var request = new QueryBuilder()
            .AddQuery("recent-high-budget-key", "movies", new QueryObject
            {
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new GreaterThanOrEqualsOperator<string>
                        {
                            Field = "release_date",
                            Value = DateTimeOffset.Parse("2025-09-19").ToString()
                        },
                        new GreaterThanOperator<int>
                        {
                            Field = "budget",
                            Value = 100000000
                        }
                    }
                },
                Sort = new List<Sort>
                {
                    new Sort
                    {
                        Field = "popularity",
                        Order = SortOrder.Desc
                    }
                },
                Facets = new List<Facet>
                {
                    new Facet
                    {
                        Field = "genres",
                        Name = "Genres"
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            })
            .AddQuery("top-rated-non-adult-key", "movies", new QueryObject
            {
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new EqualsOperator<bool>
                        {
                            Field = "adult",
                            Value = false
                        },
                        new GreaterThanOperator<int>
                        {
                            Field = "vote_average",
                            Value = 8
                        }
                    }
                },
                Sort = new List<Sort>
                {
                    new Sort
                    {
                        Field = "vote_count",
                        Order = SortOrder.Desc
                    }
                },
                Facets = new List<Facet>
                {
                    new Facet
                    {
                        Field = "production_companies",
                        Name = "production_companies"
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 5
                }
            })
            .AddQuery("movie-credits", "movieCredits", new QueryObject
            {
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 15
                }
            })
            .AddQuery("persons", "persons", new QueryObject
            {
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new EqualsOperator<string>
                        {
                            Field = "gender",
                            Value = "Female"
                        },
                        new GreaterThanOperator<DateTime>
                        {
                            Field = "birthday",
                            Value = new DateTime(1980, 1, 1)
                        }
                    }
                },
                Sort = new List<Sort>
                {
                    new Sort
                    {
                        Field = "name",
                        Order = SortOrder.Desc
                    }
                }
            })
            .AddQuery("movies-in-date-range-key", "movies", new QueryObject
            {
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new GreaterThanOrEqualsOperator<string>
                        {
                            Field = "release_date",
                            Value = DateTimeOffset.Parse("2010-01-01").ToString()
                        },
                        new LessThanOrEqualsOperator<string>
                        {
                            Field = "release_date",
                            Value = DateTimeOffset.Parse("2026-12-31").ToString()
                        }
                    }
                },
                Sort = new List<Sort>
                {
                    new Sort
                    {
                        Field = "revenue",
                        Order = SortOrder.Desc
                    }
                },
                Facets = new List<Facet>
                {
                    new Facet
                    {
                        Field = "genres",
                        Name = "Genres"
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 12
                }
            })
            .Build();

        // Execute the multi-query request and get the response
        // Here we will only get some good request to show how we can handle the response both for errors and success
        var response = await _enterspeedQueryService.Query(ApiKey, request, CancellationToken.None);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            Console.WriteLine($"Multi-query request failed with status code: {response.StatusCode}");
            return StatusCode((int)response.StatusCode, "Insufficient scope for accessing indices for current api-key");
        }

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
