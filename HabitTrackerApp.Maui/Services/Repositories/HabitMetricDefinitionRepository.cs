using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    public class HabitMetricDefinitionRepository : BaseRepository<HabitMetricDefinition>, IHabitMetricDefinitionRepository
    {
        public HabitMetricDefinitionRepository(
            IDatabaseService databaseService,
            IErrorHandlingService errorHandlingService,
            ILogger<HabitMetricDefinitionRepository> logger,
            ISyncService syncService) 
            : base(databaseService, errorHandlingService, logger, syncService)
        {
        }

        public async Task<List<HabitMetricDefinition>> GetMetricsForHabitAsync(int habitId)
        {
            try
            {
                // Get all metrics for a specific habit
                var metrics = await _databaseService.GetAllAsync<HabitMetricDefinition>();
                return metrics.Where(m => m.HabitId == habitId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for habit ID {HabitId}", habitId);
                await _errorHandlingService.HandleExceptionAsync(ex, "GetMetricsForHabit");
                return new List<HabitMetricDefinition>();
            }
        }
    }
}