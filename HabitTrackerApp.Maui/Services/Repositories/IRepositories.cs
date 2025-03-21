using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTrackerApp.Maui.Models;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    // Generic repository interface for common CRUD operations
    public interface IRepository<T> where T : IHasId, new()
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<int> SaveAsync(T item);
        Task<int> DeleteAsync(T item);
        Task<int> DeleteByIdAsync(int id);
    }

    // Habit repository interface with specialized operations
    public interface IHabitRepository : IRepository<Habit>
    {
        Task<List<Habit>> GetActiveHabitsAsync();
        Task<List<Habit>> GetHabitsByCategoryAsync(int categoryId);
        Task<Habit> GetHabitWithEntriesAsync(int habitId);
        Task<List<Habit>> SearchHabitsAsync(string searchText);
        Task<List<Habit>> GetHabitsForDateAsync(DateTime date);
    }

    // Category repository interface
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetCategoriesWithHabitsAsync();
    }

    // Daily habit entry repository interface
    public interface IDailyHabitEntryRepository : IRepository<DailyHabitEntry>
    {
        Task<List<DailyHabitEntry>> GetEntriesForHabitAsync(int habitId);
        Task<List<DailyHabitEntry>> GetEntriesForDateAsync(DateTime date);
        Task<DailyHabitEntry> GetEntryForHabitAndDateAsync(int habitId, DateTime date);
    }

    // Habit metric definition repository interface
    public interface IHabitMetricDefinitionRepository : IRepository<HabitMetricDefinition>
    {
        Task<List<HabitMetricDefinition>> GetMetricsForHabitAsync(int habitId);
    }

    // Daily metric value repository interface
    public interface IDailyMetricValueRepository : IRepository<DailyMetricValue>
    {
        Task<List<DailyMetricValue>> GetMetricValuesForEntryAsync(int entryId);
    }
}