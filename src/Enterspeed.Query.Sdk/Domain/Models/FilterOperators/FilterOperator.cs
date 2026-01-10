namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public interface IFilterOperator<TValue> : IFilterOperator
    {
        TValue Value { get; set; }
    }

    public interface IFilterOperator : IOperator
    {
        string Field { get; set; }
        string Operator { get; }
    }
}