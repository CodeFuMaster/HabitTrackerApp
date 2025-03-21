using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly IHabitRepository _habitRepository;
        
        public CategoryRepository(
            IDatabaseService databaseService,
            IErrorHandlingService errorHandlingService,
            ILogger<CategoryRepository> logger,
            ISyncService syncService,
            IHabitRepository habitRepository) 
            : base(databaseService, errorHandlingService, logger, syncService)
        {
            _habitRepository = habitRepository;
        }

        public async Task<List<Category>> GetCategoriesWithHabitsAsync()
        {
            try
            {
                // Get all categories
                var categories = await _databaseService.GetAllAsync<Category>();
                
                // Get all active habits
                var habits = await _habitRepository.GetActiveHabitsAsync();
                
                // Group habits by category
                foreach (var category in categories)
                {
                    category.Habits = habits
                        .Where(h => h.CategoryId == category.Id)
                        .ToList();
                }
                
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories with habits");
                await _errorHandlingService.HandleExceptionAsync(ex, "GetCategoriesWithHabits");
                return new List<Category>();
            }
        }
    }
}