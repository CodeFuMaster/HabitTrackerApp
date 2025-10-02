using HabitTrackerApp.SeparateAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Add CORS for cross-platform access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("*");
    });
    
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowLocalNetwork");

app.UseHttpsRedirection();

// Map controllers and SignalR hub
app.MapControllers();
app.MapHub<SyncHub>("/sync-hub");

// Health check endpoint
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Service = "HabitTracker Sync API",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow 
});

app.Run();
