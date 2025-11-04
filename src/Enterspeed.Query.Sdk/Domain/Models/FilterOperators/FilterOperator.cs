namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public abstract class FilterOperator<TValue> : FilterOperator
    {
        public abstract TValue Value { get; set; }
    }

    public abstract class FilterOperator : IOperator
    {
        public string Field { get; set; }
        public abstract string Operator { get; }
    }
}