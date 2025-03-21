using System.Collections.Generic;
using System.Threading.Tasks;

namespace HabitTrackerApp.Maui.Services
{
    public interface IDatabaseService
    {
        // Generic database operations
        Task<List<T>> GetAllAsync<T>() where T : new();
        Task<T> GetByIdAsync<T>(int id) where T : new();
        Task<int> SaveItemAsync<T>(T item);
        Task<int> DeleteItemAsync<T>(T item);
        Task<int> DeleteAllAsync<T>();
        
        // Database initialization
        Task InitializeAsync();
        
        // Transaction support
        Task<bool> BeginTransactionAsync();
        Task<bool> CommitTransactionAsync();
        Task<bool> RollbackTransactionAsync();
    }
}