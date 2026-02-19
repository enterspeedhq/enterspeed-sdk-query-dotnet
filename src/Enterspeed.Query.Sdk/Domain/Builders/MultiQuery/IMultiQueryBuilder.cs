using System;
using Enterspeed.Query.Sdk.Domain.Builders.Query;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.MultiQuery
{
    public interface IMultiQueryBuilder
    {
        MultiQueryBuilder AddQuery(string key, string index, Action<IQueryBuilder> builderAction);

        MultiQueryBuilder AddQuery<T>(string key, string index, Action<IQueryBuilder<T>> builderAction);

        MultiQueryBuilder AddQuery(string key, string index, QueryObject query);
        QueryRequest Build();

        bool ContainsKey(string key);
    }
}
