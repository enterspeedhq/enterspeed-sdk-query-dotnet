using System.Linq;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    public static class IErrorExtensions
    {
        public static bool IsForbidden(this IError error) =>
            error?.Errors != null && error.Errors.Any(e => e.Message == "Forbidden");
    }
}
