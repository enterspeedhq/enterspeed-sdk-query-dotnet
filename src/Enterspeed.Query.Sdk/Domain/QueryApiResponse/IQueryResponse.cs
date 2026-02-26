namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    public interface IQueryResponse<T>
    {
        QueryStatus Status { get; }
    }
}
