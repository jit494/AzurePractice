using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(optionsAction =>
    optionsAction.UseSqlServer(builder.Configuration["ApplicationInsights:AzureSqlConnectionString"]));
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

string Greetings = builder.Configuration["Grettings"] ?? "Hello, World!";

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching", "Rain Heavy"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/stagingtest", () =>$"Staging Test: {Greetings}");

app.MapGet("/", () => Greetings).WithName("GetGreetings").WithOpenApi();

app.MapGet("/products", async (HttpContext context, ILogger<Program> logger) =>
{
    try
    {
        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        var products = await db.Products.ToListAsync();
        logger.LogInformation("Fetched {Count} products from the database.", products.Count);
        Console.WriteLine($"Fetched {products.Count} products from the database.");
        return Results.Ok(products);
    }
    catch (Exception ex) {
        Console.WriteLine($"Error fetching products: {ex.Message}");
        return Results.Problem(ex.Message);
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
