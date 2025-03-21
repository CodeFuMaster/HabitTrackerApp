using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services
{
    public interface ILocalizationService
    {
        string GetString(string key, params object[] args);
        Task SetLanguageAsync(string languageCode);
        string CurrentLanguage { get; }
        List<LanguageOption> SupportedLanguages { get; }
        event EventHandler LanguageChanged;
    }

    public class LanguageOption
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<LocalizationService> _logger;
        private readonly List<LanguageOption> _supportedLanguages;

        public event EventHandler LanguageChanged;
        
        public LocalizationService(IStringLocalizer localizer, ILogger<LocalizationService> logger)
        {
            _localizer = localizer;
            _logger = logger;
            
            // Define supported languages
            _supportedLanguages = new List<LanguageOption>
            {
                new LanguageOption { Code = "en", Name = "English", NativeName = "English" },
                new LanguageOption { Code = "es", Name = "Spanish", NativeName = "Español" },
                new LanguageOption { Code = "fr", Name = "French", NativeName = "Français" },
                new LanguageOption { Code = "de", Name = "German", NativeName = "Deutsch" },
                new LanguageOption { Code = "ja", Name = "Japanese", NativeName = "日本語" },
                new LanguageOption { Code = "zh", Name = "Chinese", NativeName = "中文" },
            };
            
            // Load the current language from preferences
            LoadCurrentLanguage();
        }

        public string CurrentLanguage { get; private set; } = "en";
        
        public List<LanguageOption> SupportedLanguages => _supportedLanguages;

        private void LoadCurrentLanguage()
        {
            try
            {
                // Try to get saved language preference
                string savedLanguage = Preferences.Default.Get("app_language", null);
                
                if (!string.IsNullOrEmpty(savedLanguage))
                {
                    CurrentLanguage = savedLanguage;
                    
                    // Set the current thread culture based on saved preference
                    SetCurrentCulture(savedLanguage);
                }
                else
                {
                    // If no preference saved, get device language
                    var deviceCulture = CultureInfo.CurrentCulture;
                    var deviceLanguage = deviceCulture.TwoLetterISOLanguageName;
                    
                    // If device language is supported, use it
                    if (_supportedLanguages.Exists(l => l.Code == deviceLanguage))
                    {
                        CurrentLanguage = deviceLanguage;
                    }
                    else
                    {
                        // Default to English if device language not supported
                        CurrentLanguage = "en";
                    }
                    
                    // Save the language preference
                    Preferences.Default.Set("app_language", CurrentLanguage);
                }
                
                _logger.LogInformation("Current language set to: {Language}", CurrentLanguage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading language settings");
                
                // Default to English on error
                CurrentLanguage = "en";
            }
        }

        public string GetString(string key, params object[] args)
        {
            try
            {
                var localizedString = _localizer[key];
                
                if (args != null && args.Length > 0)
                {
                    return string.Format(localizedString.Value, args);
                }
                
                return localizedString.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Missing localization key: {Key}", key);
                return key; // Return the key itself if no translation found
            }
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) || 
                languageCode == CurrentLanguage || 
                !_supportedLanguages.Exists(l => l.Code == languageCode))
            {
                return;
            }

            try
            {
                // Save language preference
                Preferences.Default.Set("app_language", languageCode);
                CurrentLanguage = languageCode;
                
                // Set current culture
                SetCurrentCulture(languageCode);
                
                // Notify subscribers that language changed
                LanguageChanged?.Invoke(this, EventArgs.Empty);
                
                _logger.LogInformation("Language changed to: {Language}", languageCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting language to: {Language}", languageCode);
            }
        }
        
        private void SetCurrentCulture(string languageCode)
        {
            var cultureInfo = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}