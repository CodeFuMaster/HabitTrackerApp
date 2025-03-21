using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    public class DailyHabitEntryRepository : BaseRepository<DailyHabitEntry>, IDailyHabitEntryRepository
    {
        public DailyHabitEntryRepository(
            IDatabaseService databaseService,
            IErrorHandlingService errorHandlingService,
            ILogger<DailyHabitEntryRepository> logger,
            ISyncService syncService) 
            : base(databaseService, errorHandlingService, logger, syncService)
        {
        }

        public async Task<List<DailyHabitEntry>> GetEntriesForHabitAsync(int habitId)
        {
            try
            {
                // Get all entries for a specific habit
                var entries = await _databaseService.GetAllAsync<DailyHabitEntry>();
                return entries.Where(e => e.HabitId == habitId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entries for habit ID {HabitId}", habitId);
                await _errorHandlingService.HandleExceptionAsync(ex, "GetEntriesForHabit");
                return new List<DailyHabitEntry>();
            }
        }

        public async Task<List<DailyHabitEntry>> GetEntriesForDateAsync(DateTime date)
        {
            try
            {
                // Normalize date to remove time component
                var normalizedDate = date.Date;
                
                // Get all entries for a specific date
                var entries = await _databaseService.GetAllAsync<DailyHabitEntry>();
                return entries.Where(e => e.Date.Date == normalizedDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entries for date {Date}", date.ToString("yyyy-MM-dd"));
                await _errorHandlingService.HandleExceptionAsync(ex, "GetEntriesForDate");
                return new List<DailyHabitEntry>();
            }
        }

        public async Task<DailyHabitEntry> GetEntryForHabitAndDateAsync(int habitId, DateTime date)
        {
            try
            {
                // Normalize date to remove time component
                var normalizedDate = date.Date;
                
                // Get entry for a specific habit and date
                var entries = await _databaseService.GetAllAsync<DailyHabitEntry>();
                return entries.FirstOrDefault(e => e.HabitId == habitId && e.Date.Date == normalizedDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entry for habit ID {HabitId} and date {Date}", 
                    habitId, date.ToString("yyyy-MM-dd"));
                await _errorHandlingService.HandleExceptionAsync(ex, "GetEntryForHabitAndDate");
                return null;
            }
        }
        
        public override async Task<int> SaveAsync(DailyHabitEntry entry)
        {
            try
            {
                // Ensure date is normalized
                entry.Date = entry.Date.Date;
                
                // Set updated timestamp
                entry.UpdatedAt = DateTimeOffset.Now;
                
                // For new entries, set checked timestamp
                if (entry.Id == 0)
                {
                    entry.CheckedAt = DateTimeOffset.Now;
                }
                
                return await base.SaveAsync(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving habit entry for habit ID {HabitId} and date {Date}", 
                    entry.HabitId, entry.Date.ToString("yyyy-MM-dd"));
                await _errorHandlingService.HandleExceptionAsync(ex, "SaveHabitEntry");
                return 0;
            }
        }
    }
}