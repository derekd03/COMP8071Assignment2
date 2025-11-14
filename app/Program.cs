using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Read connection strings
string oltpConnStr = builder.Configuration.GetConnectionString("OLTPConnection")!;
string olapConnStr = builder.Configuration.GetConnectionString("OLAPConnection")!;

Console.WriteLine("Database connections configured");
Console.WriteLine("Visit http://localhost:5292/api/etl/run to run OLTP -> ETL -> OLAP.");

// Register services
builder.Services.AddSingleton<ETLService>(sp => new ETLService(oltpConnStr, olapConnStr));
builder.Services.AddSingleton<OLTPSeedService>(sp => new OLTPSeedService(oltpConnStr));
builder.Services.AddScoped<OracleConnection>(sp => new OracleConnection(oltpConnStr));

var app = builder.Build();

// Seed the OLTP database at startup
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<OLTPSeedService>();
    await seedService.SeedIfEmptyAsync("SeedData/CareServicesOLTPInsertion.sql");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseStaticFiles();
app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.Run();