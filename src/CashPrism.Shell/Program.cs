using CashPrism.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCashPrismWeb();

var app = builder.Build();

app.MapCashPrismWeb();

app.Run();

/// <summary>
/// Exposed so the integration tests can host the composition root through
/// <c>WebApplicationFactory&lt;Program&gt;</c>.
/// </summary>
public partial class Program;
