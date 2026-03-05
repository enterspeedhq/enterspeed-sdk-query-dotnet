using System;
using Enterspeed.Query.Sdk.Domain.Builders.Query;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.MultiQuery
{
    public interface IQueryBuilder
    {
        QueryBuilder AddQuery(string key, string index, Action<IQuery> builderAction);

        QueryBuilder AddQuery<T>(string key, string index, Action<IQuery<T>> builderAction);

        QueryBuilder AddQuery(string key, string index, QueryObject query);
        QueryRequest Build();

        bool ContainsKey(string key);
    }
}
