namespace Enterspeed.Query.Sdk.Api.Services
{
    public interface IJsonSerializer
    {
        string Serialize(object value);
        T Deserialize<T>(string value);
    }
}