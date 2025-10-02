using HabitTrackerApp.API.Hubs;
using HabitTrackerApp.Core.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure SQLite database for cross-platform sync
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "habittracker_sync.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<HabitTrackerDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Configure SignalR for real-time sync
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configure CORS for local network access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5000", 
            "http://192.168.*",
            "http://10.0.*"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HabitTrackerDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    app.Logger.LogInformation("Database initialized at: {DbPath}", dbPath);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalNetwork");

app.UseRouting();
app.UseAuthorization();

// Map controllers and SignalR hub
app.MapControllers();
app.MapHub<SyncHub>("/synchub");

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy",
    timestamp = DateTime.UtcNow,
    server = "HabitTracker Sync API",
    version = "1.0.0"
}));

app.Logger.LogInformation("HabitTracker Sync API started successfully");
app.Logger.LogInformation("SignalR Hub available at: /synchub");
app.Logger.LogInformation("Sync API available at: /api/sync");

app.Run();
