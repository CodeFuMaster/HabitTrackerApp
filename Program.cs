
using HabitTrackerApp.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the DbContext to use SQL Server.
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Register repositories
//builder.Services.AddScoped<IHabitRepository, EfHabitRepository>();
//builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
//builder.Services.AddScoped<IMetricsRepository, EfMetricsRepository>();

//// Register services
//builder.Services.AddScoped<HabitService>();
//builder.Services.AddScoped<CategoryService>();
//builder.Services.AddScoped<MetricsService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
