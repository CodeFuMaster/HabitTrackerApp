using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HabitTrackerApp.Maui.Models;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services
{
    public class SQLiteDatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly ILogger<SQLiteDatabaseService> _logger;
        private bool _isInitialized = false;

        public SQLiteDatabaseService(ILogger<SQLiteDatabaseService> logger)
        {
            _logger = logger;
        }

        private async Task InitializeDatabaseAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                if (_database != null)
                    return;

                // Get the database path
                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "HabitTracker.db");
                _logger.LogInformation($"Database path: {databasePath}");

                // Create the connection
                _database = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                // Create all tables
                await _database.CreateTableAsync<Category>();
                await _database.CreateTableAsync<Habit>();
                await _database.CreateTableAsync<DailyHabitEntry>();
                await _database.CreateTableAsync<DailyMetricValue>();
                await _database.CreateTableAsync<HabitMetricDefinition>();

                _logger.LogInformation("Database initialized successfully");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                throw;
            }
        }

        public async Task InitializeAsync()
        {
            await InitializeDatabaseAsync();
        }

        public async Task<List<T>> GetAllAsync<T>() where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync<T>(int id) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.FindAsync<T>(id);
        }

        public async Task<int> SaveItemAsync<T>(T item)
        {
            await InitializeDatabaseAsync();
            
            // Check if the item already exists (has an ID > 0)
            if (item is IHasId model && model.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteItemAsync<T>(T item)
        {
            await InitializeDatabaseAsync();
            return await _database.DeleteAsync(item);
        }

        public async Task<int> DeleteAllAsync<T>()
        {
            await InitializeDatabaseAsync();
            return await _database.DeleteAllAsync<T>();
        }

        public async Task<bool> BeginTransactionAsync()
        {
            await InitializeDatabaseAsync();
            await _database.ExecuteAsync("BEGIN TRANSACTION");
            return true;
        }

        public async Task<bool> CommitTransactionAsync()
        {
            await _database.ExecuteAsync("COMMIT");
            return true;
        }

        public async Task<bool> RollbackTransactionAsync()
        {
            await _database.ExecuteAsync("ROLLBACK");
            return true;
        }
    }

    // Interface for models that have an ID
    public interface IHasId
    {
        int Id { get; set; }
    }
}