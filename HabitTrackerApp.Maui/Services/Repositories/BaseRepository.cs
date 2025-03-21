using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace HabitTrackerApp.Maui.Services.Repositories
{
    // Base repository implementation that provides common CRUD operations
    public abstract class BaseRepository<T> : IRepository<T> where T : IHasId, new()
    {
        protected readonly IDatabaseService _databaseService;
        protected readonly IErrorHandlingService _errorHandlingService;
        protected readonly ILogger _logger;
        protected readonly ISyncService _syncService;

        public BaseRepository(
            IDatabaseService databaseService,
            IErrorHandlingService errorHandlingService,
            ILogger logger,
            ISyncService syncService)
        {
            _databaseService = databaseService;
            _errorHandlingService = errorHandlingService;
            _logger = logger;
            _syncService = syncService;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                return await _databaseService.GetAllAsync<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {EntityType}", typeof(T).Name);
                await _errorHandlingService.HandleExceptionAsync(ex, $"Get{typeof(T).Name}List");
                return new List<T>();
            }
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            try
            {
                return await _databaseService.GetByIdAsync<T>(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {EntityType} with ID {Id}", typeof(T).Name, id);
                await _errorHandlingService.HandleExceptionAsync(ex, $"Get{typeof(T).Name}");
                return default;
            }
        }

        public virtual async Task<int> SaveAsync(T item)
        {
            try
            {
                int result = await _databaseService.SaveItemAsync(item);
                
                // Queue change for synchronization with the server
                if (result > 0)
                {
                    ChangeType changeType = item.Id == 0 ? ChangeType.Create : ChangeType.Update;
                    await _syncService.QueueChangeAsync(item, changeType);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving {EntityType} with ID {Id}", typeof(T).Name, item.Id);
                await _errorHandlingService.HandleExceptionAsync(ex, $"Save{typeof(T).Name}");
                return 0;
            }
        }

        public virtual async Task<int> DeleteAsync(T item)
        {
            try
            {
                int result = await _databaseService.DeleteItemAsync(item);
                
                // Queue change for synchronization with the server
                if (result > 0)
                {
                    await _syncService.QueueChangeAsync(item, ChangeType.Delete);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(T).Name, item.Id);
                await _errorHandlingService.HandleExceptionAsync(ex, $"Delete{typeof(T).Name}");
                return 0;
            }
        }

        public virtual async Task<int> DeleteByIdAsync(int id)
        {
            var item = await GetByIdAsync(id);
            if (item != null)
            {
                return await DeleteAsync(item);
            }
            return 0;
        }
    }
}