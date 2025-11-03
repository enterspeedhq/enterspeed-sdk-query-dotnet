using System.Collections.Generic;
using System.Linq;
using Enterspeed.Query.Sdk.Api.Models;

namespace Enterspeed.Query.Sdk.Api.Extensions
{
    public static class ContentListExtensions
    {
        public static List<T> GetContent<T>(this List<IContent> contents)
        {
            return contents.Select(x => x.GetContent<T>()).ToList();
        }
    }
}