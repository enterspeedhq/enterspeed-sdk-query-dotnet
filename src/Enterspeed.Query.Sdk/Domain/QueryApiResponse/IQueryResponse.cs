namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
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
