# [Enterspeed Query .NET SDK](https://www.enterspeed.com/) &middot; [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](./LICENSE) [![NuGet version](https://img.shields.io/nuget/v/Enterspeed.Query.Sdk)](https://www.nuget.org/packages/Enterspeed.Query.Sdk/) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/enterspeedhq/enterspeed-sdk-query-dotnet/pulls) [![Twitter](https://img.shields.io/twitter/follow/enterspeedhq?style=social)](https://twitter.com/enterspeedhq)

## Installation

With .NET CLI

```bash
dotnet add package Enterspeed.Query.Sdk --version <version>
```

With Package Manager

```bash
Install-Package Enterspeed.Query.Sdk -Version <version>
```
## How to use

### Register services
Service has to be added to the service collection. This can be one by using the following extension method.
```c#
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddEnterspeedQueryService())
    .Build();
```
### Examples of usage
Example of a common implementation where the query service is being utilized.
```c#
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Services;

namespace Query; 

public class MyQueryService
{
    private readonly IEnterspeedQueryService _enterspeedQueryService;
    
    public MyQueryService(IEnterspeedQueryService enterspeedQueryService)
    {
        _enterspeedQueryService = enterspeedQueryService;
    }

    // Query an Enterspeed index 
    public async Task<QueryApiResponse> Query()
    {
        var query = new QueryObject
        {
            Sort = new List<Sort>
            {
                new Sort()
                {
                    Field = "_updatedAt",
                    Order = SortOrder.Desc
                }
            },
            Facets = new List<Facet>
            {
                new Facet()
                {
                    Field = "tags",
                    Name = "Tags",
                    Size = 3
                }
            },
            Filters = new AndOperator
            {
                And = new List<IOperator>
                {
                    new EqualsOperator<string>
                    {
                        Field = "isActive",
                        Value = true
                    },
                    new OrOperator
                    {
                        Or = new List<IOperator>
                        {
                            new EqualsOperator<string>
                            {
                                Field = "isGlobal",
                                Value = true
                            },
                            new InOperator<string>
                            {
                                Field = "category",
                                Value = new List<string> { "selected category 1", "selected category 2" }
                            }
                        }
                    }
                }
            },
            Aliases = new List<string> { "blogPostTile" },
            Pagination = new Pagination
            {
                Page = 0,
                PageSize = 10
            }
        };

        var response = await enterspeedQueryService.Query("environment-******-****-****-****-**********", "blogIndex", query);

        return response;
    }

    // Query an Enterspeed index and get strongly typed result
    public async Task<List<BlogPost>> QueryTyped()
    {
        var typedResult = await enterspeedQueryService.QueryTypd("environment-******-****-****-****-**********", "blogIndex", new QueryObject());

        List<BlogPost> blogPosts = typedResult.Response.Results.GetContent<BlogPost>();

        return blogPosts;
    }

    // Multiple queries against an Enterspeed index 
    public async Task<MultiQueryApiResponse> MultiQuery()
    {
        var multiQuery = new List<MultiQueryObject>{
            new()
            {
                Index = "blogIndex",
                Name = "blogPosts",
                Filters = new AndOperator
                {
                    And = new List<IOperator>
                    {
                        new EqualsOperator<string>
                        {
                            Field = "title",
                            Value = "My Blog Post",
                            CaseInsensitive = true
                        }
                    }
                }
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            },
            new()
            {
                Index = "blogIndex",
                Name = "allCategoryFacets",
                Facets = new List<Facet>
                {
                    new()
                    {
                        Field = "category",
                        Name = "Category"
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 0
                }
            }
        }

        var response = await enterspeedQueryService.Query("environment-******-****-****-****-**********", multiQuery);

        var blogPostsQueryResponse = multipleResult.Response.Single(x => x.Name == "blogPosts");
        var allFacetsQueryResponse = multipleResult.Response.Single(x => x.Name == "allFacets");

        return response;
    }

    // Multiple queries against an Enterspeed index and get strongly typed results
    public async Task<MultiQueryApiResponse> MultiQueryTyped()
    {
        var multiQuery = new List<MultiQueryObject>{
            new()
            {
                Index = "blogIndex",
                Name = "blogPosts",
                Sort = new List<Sort>
                {
                    new()
                    {
                        Field = "_updatedAt",
                        Order = SortOrder.Desc
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            },
            new()
            {
                Index = "productIndex",
                Name = "products",
                Sort = new List<Sort>
                {
                    new()
                    {
                        Field = "_updatedAt",
                        Order = SortOrder.Desc
                    }
                },
                Pagination = new Pagination
                {
                    Page = 0,
                    PageSize = 10
                }
            }
        }

        var response = await enterspeedQueryService.QueryTyped("environment-******-****-****-****-**********", multiQuery);

        List<BlogPost> blogPosts = multipleResult.Response.Single(x => x.Name == "blogPosts").Results.GetContent<BlogPost>();
        List<Product> products = multipleResult.Response.Single(x => x.Name == "allFacets").Results.GetContent<Product>();

        return response;
    }

    public class BlogPost {
        [JsonPropertyName("date")]
        public DateTime PublishedDate { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string[] Tags { get; set; }
    }

    public class Product {
        public string Url { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public string Sku { get; set; }
        public string Category { get; set; }
    }
}
```


## Contributing

Pull requests are very welcome.  
Please fork this repository and make a PR when you are ready.  

Otherwise you are welcome to open an Issue in our [issue tracker](https://github.com/enterspeedhq/enterspeed-sdk-query-dotnet/issues).

## License

Enterspeed .NET SDK is [MIT licensed](./LICENSE)
