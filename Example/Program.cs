using Enterspeed.Query.Sdk.Api.Extensions;

using Example;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddControllers();
builder.Services.AddEnterspeedQueryService();

var app = builder.Build();

app.MapControllers();
app.Run();
