using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Register ETL service
builder.Services.AddSingleton(new ETLService(oltpConnStr, olapConnStr));
builder.Services.AddScoped<OracleConnection>(provider => new OracleConnection(oltpConnStr));

var app = builder.Build();

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