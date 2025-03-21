using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using HabitTrackerApp.Maui.Services;

namespace HabitTrackerApp.Maui.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        protected readonly IErrorHandlingService ErrorHandlingService;
        protected readonly ILogger Logger;
        
        private bool _isBusy;
        private string _title;
        private bool _isRefreshing;
        private string _errorMessage;
        
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        public BaseViewModel(IErrorHandlingService errorHandlingService, ILogger logger)
        {
            ErrorHandlingService = errorHandlingService;
            Logger = logger;
        }
        
        /// <summary>
        /// Safely executes an async operation with proper error handling and busy state management
        /// </summary>
        protected async Task SafeExecuteAsync(Func<Task> operation, string operationName = "", bool showBusyIndicator = true)
        {
            try
            {
                if (showBusyIndicator)
                {
                    IsBusy = true;
                }
                
                ErrorMessage = string.Empty;
                
                Logger.LogInformation("Starting operation: {OperationName}", 
                    string.IsNullOrEmpty(operationName) ? "Unnamed operation" : operationName);
                
                await operation();
            }
            catch (TaskCanceledException)
            {
                Logger.LogInformation("Operation cancelled: {OperationName}", operationName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in {OperationName}: {ErrorMessage}", 
                    operationName, ex.Message);
                await ErrorHandlingService.HandleExceptionAsync(ex, operationName);
                ErrorMessage = ErrorHandlingService.GetUserFriendlyErrorMessage(ex);
            }
            finally
            {
                if (showBusyIndicator)
                {
                    IsBusy = false;
                }
                
                IsRefreshing = false;
            }
        }
        
        /// <summary>
        /// Safely executes an async operation that returns a value, with proper error handling and busy state management
        /// </summary>
        protected async Task<TResult> SafeExecuteAsync<TResult>(
            Func<Task<TResult>> operation, 
            TResult defaultValue = default, 
            string operationName = "", 
            bool showBusyIndicator = true)
        {
            try
            {
                if (showBusyIndicator)
                {
                    IsBusy = true;
                }
                
                ErrorMessage = string.Empty;
                
                Logger.LogInformation("Starting operation: {OperationName}", 
                    string.IsNullOrEmpty(operationName) ? "Unnamed operation" : operationName);
                
                return await operation();
            }
            catch (TaskCanceledException)
            {
                Logger.LogInformation("Operation cancelled: {OperationName}", operationName);
                return defaultValue;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in {OperationName}: {ErrorMessage}", 
                    operationName, ex.Message);
                await ErrorHandlingService.HandleExceptionAsync(ex, operationName);
                ErrorMessage = ErrorHandlingService.GetUserFriendlyErrorMessage(ex);
                return defaultValue;
            }
            finally
            {
                if (showBusyIndicator)
                {
                    IsBusy = false;
                }
                
                IsRefreshing = false;
            }
        }
        
        /// <summary>
        /// Creates a relay command that safely executes an async operation
        /// </summary>
        protected AsyncRelayCommand CreateCommand(
            Func<Task> execute, 
            string operationName = "", 
            bool showBusyIndicator = true)
        {
            return new AsyncRelayCommand(
                async () => await SafeExecuteAsync(execute, operationName, showBusyIndicator),
                () => !IsBusy);
        }
        
        /// <summary>
        /// Creates a relay command with a parameter that safely executes an async operation
        /// </summary>
        protected AsyncRelayCommand<T> CreateCommand<T>(
            Func<T, Task> execute, 
            string operationName = "", 
            bool showBusyIndicator = true,
            Predicate<T> canExecute = null)
        {
            return new AsyncRelayCommand<T>(
                async (param) => await SafeExecuteAsync(() => execute(param), operationName, showBusyIndicator),
                (param) => !IsBusy && (canExecute?.Invoke(param) ?? true));
        }
    }
}