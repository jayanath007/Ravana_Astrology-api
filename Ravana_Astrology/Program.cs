using Ravana_Astrology.Configuration;
using Ravana_Astrology.Services.Implementation;
using Ravana_Astrology.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Swiss Ephemeris options
builder.Services.Configure<SwissEphemerisOptions>(
    builder.Configuration.GetSection("SwissEphemeris"));

// Register services
builder.Services.AddScoped<IAstrologyCalculationService, AstrologyCalculationService>();
builder.Services.AddScoped<IVimshottariDashaService, VimshottariDashaService>();

// Configure CORS to allow all origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS - must be before other middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
