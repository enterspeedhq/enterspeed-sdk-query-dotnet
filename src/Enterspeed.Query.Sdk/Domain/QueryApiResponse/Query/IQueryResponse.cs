namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    public interface IQueryResponse
    {
       QueryStatus Status { get; }
    }

    public interface IQueryResponse<T>
    {
        QueryStatus Status { get; }
    }
}
