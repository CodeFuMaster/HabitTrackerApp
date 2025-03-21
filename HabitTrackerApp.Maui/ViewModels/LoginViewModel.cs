using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Maui.Services;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthenticationService _authService;
        
        [ObservableProperty]
        private string _username;
        
        [ObservableProperty]
        private string _password;
        
        [ObservableProperty]
        private bool _rememberMe;
        
        [ObservableProperty]
        private bool _isRegistering;
        
        [ObservableProperty]
        private string _email;
        
        [ObservableProperty]
        private string _confirmPassword;
        
        public LoginViewModel(
            IAuthenticationService authService,
            IErrorHandlingService errorHandlingService,
            ILogger<LoginViewModel> logger) 
            : base(errorHandlingService, logger)
        {
            _authService = authService;
            Title = "Login";
            
            // Try to restore saved username if available
            Username = Preferences.Default.Get("saved_username", string.Empty);
            RememberMe = !string.IsNullOrEmpty(Username);
        }
        
        [RelayCommand]
        private async Task Login()
        {
            await SafeExecuteAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required.";
                    return;
                }
                
                Logger.LogInformation("Attempting to login user: {Username}", Username);
                
                var success = await _authService.LoginAsync(Username, Password);
                if (success)
                {
                    Logger.LogInformation("Login successful for user: {Username}", Username);
                    
                    // Save username if remember me is checked
                    if (RememberMe)
                    {
                        Preferences.Default.Set("saved_username", Username);
                    }
                    else
                    {
                        Preferences.Default.Remove("saved_username");
                    }
                    
                    // Navigate to main page after successful login
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    Logger.LogWarning("Login failed for user: {Username}", Username);
                    ErrorMessage = "Invalid username or password. Please try again.";
                }
            }, "LoginOperation");
        }
        
        [RelayCommand]
        private async Task Register()
        {
            if (!IsRegistering)
            {
                // Toggle to registration view
                IsRegistering = true;
                Title = "Register";
                return;
            }
            
            await SafeExecuteAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || 
                    string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    ErrorMessage = "All fields are required.";
                    return;
                }
                
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    return;
                }
                
                Logger.LogInformation("Attempting to register user: {Username}, {Email}", Username, Email);
                
                var success = await _authService.RegisterAsync(Username, Email, Password);
                if (success)
                {
                    Logger.LogInformation("Registration successful for user: {Username}", Username);
                    
                    // Automatically login after successful registration
                    await Login();
                }
                else
                {
                    Logger.LogWarning("Registration failed for user: {Username}", Username);
                    ErrorMessage = "Registration failed. The username or email may already be in use.";
                }
            }, "RegisterOperation");
        }
        
        [RelayCommand]
        private void ToggleRegistration()
        {
            IsRegistering = !IsRegistering;
            ErrorMessage = string.Empty;
            
            if (IsRegistering)
            {
                Title = "Register";
            }
            else
            {
                Title = "Login";
            }
        }
    }
}