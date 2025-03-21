using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    public class HabitRepository : BaseRepository<Habit>, IHabitRepository
    {
        private readonly IDailyHabitEntryRepository _dailyHabitEntryRepository;
        
        public HabitRepository(
            IDatabaseService databaseService,
            IErrorHandlingService errorHandlingService,
            ILogger<HabitRepository> logger,
            ISyncService syncService,
            IDailyHabitEntryRepository dailyHabitEntryRepository) 
            : base(databaseService, errorHandlingService, logger, syncService)
        {
            _dailyHabitEntryRepository = dailyHabitEntryRepository;
        }

        public async Task<List<Habit>> GetActiveHabitsAsync()
        {
            try
            {
                // Get all habits that aren't marked as deleted
                var habits = await _databaseService.GetAllAsync<Habit>();
                return habits.Where(h => !h.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active habits");
                await _errorHandlingService.HandleExceptionAsync(ex, "GetActiveHabits");
                return new List<Habit>();
            }
        }

        public async Task<List<Habit>> GetHabitsByCategoryAsync(int categoryId)
        {
            try
            {
                // Get all habits for a specific category
                var habits = await _databaseService.GetAllAsync<Habit>();
                return habits.Where(h => !h.IsDeleted && h.CategoryId == categoryId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting habits for category {CategoryId}", categoryId);
                await _errorHandlingService.HandleExceptionAsync(ex, "GetHabitsByCategory");
                return new List<Habit>();
            }
        }

        public async Task<Habit> GetHabitWithEntriesAsync(int habitId)
        {
            try
            {
                // Get the habit
                var habit = await _databaseService.GetByIdAsync<Habit>(habitId);
                if (habit == null)
                    return null;

                // Get all entries for this habit
                habit.DailyHabitEntries = await _dailyHabitEntryRepository.GetEntriesForHabitAsync(habitId);
                
                return habit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting habit with entries for habit ID {HabitId}", habitId);
                await _errorHandlingService.HandleExceptionAsync(ex, "GetHabitWithEntries");
                return null;
            }
        }

        public async Task<List<Habit>> SearchHabitsAsync(string searchText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                    return await GetActiveHabitsAsync();

                // Get all active habits
                var habits = await GetActiveHabitsAsync();
                
                // Filter based on search term (case-insensitive)
                searchText = searchText.ToLower();
                return habits.Where(h => 
                    h.Name.ToLower().Contains(searchText) || 
                    (h.Description != null && h.Description.ToLower().Contains(searchText)) ||
                    (h.Tags != null && h.Tags.ToLower().Contains(searchText))
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching habits with term '{SearchText}'", searchText);
                await _errorHandlingService.HandleExceptionAsync(ex, "SearchHabits");
                return new List<Habit>();
            }
        }

        public async Task<List<Habit>> GetHabitsForDateAsync(DateTime date)
        {
            try
            {
                // Get all active habits
                var habits = await GetActiveHabitsAsync();
                
                // Return only habits that should occur on the specified date
                return habits.Where(h => h.OccursOn(date)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting habits for date {Date}", date.ToString("yyyy-MM-dd"));
                await _errorHandlingService.HandleExceptionAsync(ex, "GetHabitsForDate");
                return new List<Habit>();
            }
        }
    }
}