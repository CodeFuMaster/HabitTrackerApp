
using HabitTrackerApp.Data;
using HabitTrackerApp.Core.Data;
using HabitTrackerApp.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Add CORS support for MAUI app
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMAUI", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure the original DbContext (keep for legacy compatibility)
// Using SQLite instead of PostgreSQL for simplicity
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=habittracker.db"));

// Configure the new enhanced SQLite DbContext for cross-platform
builder.Services.AddDbContext<HabitTrackerDbContext>(options =>
    options.UseSqlite("Data Source=habittracker.db"));

// Register enhanced services
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<OfflineSyncService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable CORS for mobile app
app.UseCors("AllowMAUI");

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Habit}/{action=Index}/{id?}")
    .WithStaticAssets();

// Keep Home route for direct access
app.MapControllerRoute(
    name: "home",
    pattern: "Home/{action=Index}/{id?}",
    defaults: new { controller = "Home" });


app.Run();
