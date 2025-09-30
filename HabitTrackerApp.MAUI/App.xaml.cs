using HabitTrackerApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Core.Models;

namespace HabitTrackerApp.MAUI;

public partial class App : Application
{
	public App(HabitTrackerDbContext dbContext)
	{
		InitializeComponent();
		
		// Ensure database is created
		InitializeDatabaseAsync(dbContext);
	}

	private async void InitializeDatabaseAsync(HabitTrackerDbContext dbContext)
	{
		try
		{
			// Ensure database is created
			await dbContext.Database.EnsureCreatedAsync();
			
			// Run any pending migrations
			if (dbContext.Database.GetPendingMigrations().Any())
			{
				await dbContext.Database.MigrateAsync();
			}
			
			// Seed initial data if empty
			await SeedInitialDataAsync(dbContext);
		}
		catch (Exception ex)
		{
			// Log error but don't crash the app
			System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
		}
	}
	
	private async Task SeedInitialDataAsync(HabitTrackerDbContext dbContext)
	{
		// Check if we already have data
		if (await dbContext.Habits.AnyAsync())
			return;
			
		// Add sample categories
		var healthCategory = new HabitTrackerApp.Core.Models.Category 
		{ 
			Name = "Health", 
			Description = "Health and fitness habits",
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		var learningCategory = new HabitTrackerApp.Core.Models.Category 
		{ 
			Name = "Learning", 
			Description = "Education and skill development",
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		var productivityCategory = new HabitTrackerApp.Core.Models.Category 
		{ 
			Name = "Productivity", 
			Description = "Work and productivity habits",
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		dbContext.Categories.AddRange(healthCategory, learningCategory, productivityCategory);
		await dbContext.SaveChangesAsync();
		
		// Add sample habits
		var waterHabit = new HabitTrackerApp.Core.Models.Habit 
		{ 
			Name = "Drink Water", 
			Description = "Drink 8 glasses of water daily", 
			CategoryId = healthCategory.Id, 
			IsActive = true,
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		var exerciseHabit = new HabitTrackerApp.Core.Models.Habit 
		{ 
			Name = "Exercise", 
			Description = "30 minutes of physical activity", 
			CategoryId = healthCategory.Id, 
			IsActive = true,
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		var readingHabit = new HabitTrackerApp.Core.Models.Habit 
		{ 
			Name = "Read Books", 
			Description = "Read for 30 minutes", 
			CategoryId = learningCategory.Id, 
			IsActive = true,
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		var planningHabit = new HabitTrackerApp.Core.Models.Habit 
		{ 
			Name = "Plan Daily Tasks", 
			Description = "Plan tomorrow's tasks", 
			CategoryId = productivityCategory.Id, 
			IsActive = true,
			CreatedDate = DateTimeOffset.Now,
			LastModifiedDate = DateTimeOffset.Now
		};
		
		dbContext.Habits.AddRange(waterHabit, exerciseHabit, readingHabit, planningHabit);
		await dbContext.SaveChangesAsync();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}