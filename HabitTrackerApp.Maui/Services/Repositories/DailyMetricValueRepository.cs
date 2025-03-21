using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    public class DailyMetricValueRepository : BaseRepository<DailyMetricValue>, IDailyMetricValueRepository
    {
        public DailyMetricValueRepository(
            IDatabaseService databaseService,
            IErrorHandlingService errorHandlingService,
            ILogger<DailyMetricValueRepository> logger,
            ISyncService syncService) 
            : base(databaseService, errorHandlingService, logger, syncService)
        {
        }

        public async Task<List<DailyMetricValue>> GetMetricValuesForEntryAsync(int entryId)
        {
            try
            {
                // Get all metric values for a specific daily habit entry
                var values = await _databaseService.GetAllAsync<DailyMetricValue>();
                return values.Where(v => v.DailyHabitEntryId == entryId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metric values for entry ID {EntryId}", entryId);
                await _errorHandlingService.HandleExceptionAsync(ex, "GetMetricValuesForEntry");
                return new List<DailyMetricValue>();
            }
        }
    }
}