using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models
{
    public class FacetResult
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public List<FacetGroup> Groups { get; set; }
        public bool IsValid { get; set; }
    }
}