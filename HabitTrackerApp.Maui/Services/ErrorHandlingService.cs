using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HabitTrackerApp.Maui.Services
{
    public interface IErrorHandlingService
    {
        // Handle exceptions and provide appropriate responses
        Task<bool> HandleExceptionAsync(Exception ex, string context = "");
        
        // Log handled exceptions
        void LogException(Exception ex, string context = "");
        
        // Get user-friendly error message
        string GetUserFriendlyErrorMessage(Exception ex);
    }
    
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        
        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> HandleExceptionAsync(Exception ex, string context = "")
        {
            // Log the exception
            LogException(ex, context);
            
            // Show error to user (using MAUI dialog)
            string userMessage = GetUserFriendlyErrorMessage(ex);
            await Shell.Current.DisplayAlert("Error", userMessage, "OK");
            
            // Return false to indicate error was handled
            return false;
        }
        
        public void LogException(Exception ex, string context = "")
        {
            // Use structured logging with Serilog
            _logger.LogError(ex, "Error occurred in {Context}: {ErrorMessage}", 
                context, ex.Message);
        }
        
        public string GetUserFriendlyErrorMessage(Exception ex)
        {
            // Convert technical exceptions to user-friendly messages
            if (ex is SQLite.SQLiteException sqlEx)
            {
                return "There was a problem accessing the database. Please try again later.";
            }
            else if (ex is System.Net.Http.HttpRequestException)
            {
                return "Unable to connect to the server. Please check your internet connection and try again.";
            }
            else if (ex is TaskCanceledException)
            {
                return "The operation timed out. Please try again later.";
            }
            else if (ex is UnauthorizedAccessException)
            {
                return "You don't have permission to perform this action. Please log in again.";
            }
            else if (ex is ArgumentException)
            {
                return "Invalid information provided. Please check your inputs and try again.";
            }
            
            // For unknown exceptions, provide a generic message
            return "An unexpected error occurred. Please try again later.";
        }
    }
}