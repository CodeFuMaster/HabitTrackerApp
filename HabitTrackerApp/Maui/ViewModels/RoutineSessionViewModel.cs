using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using HabitTrackerMobile.Models;
using HabitTrackerMobile.Services;

namespace HabitTrackerMobile.ViewModels;

/// <summary>
/// Routine Session ViewModel - For tracking complex routines like gym sessions
/// </summary>
public partial class RoutineSessionViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabaseService _localDatabase;
    private readonly ISyncService _syncService;
    private readonly IDispatcherTimer _timer;

    [ObservableProperty]
    private RoutineSession? currentSession;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isSessionActive;

    [ObservableProperty]
    private string sessionDuration = "00:00:00";

    [ObservableProperty]
    private ObservableCollection<SessionActivity> activities = new();

    [ObservableProperty]
    private ObservableCollection<ActivityTemplate> availableTemplates = new();

    [ObservableProperty]
    private SessionActivity? selectedActivity;

    [ObservableProperty]
    private bool isAddingActivity;

    [ObservableProperty]
    private string newActivityName = string.Empty;

    [ObservableProperty]
    private ActivityTemplate? selectedTemplate;

    // Timer for current activity
    [ObservableProperty]
    private SessionActivity? activeActivity;

    [ObservableProperty]
    private string activityDuration = "00:00";

    public RoutineSessionViewModel(IApiService apiService, ILocalDatabaseService localDatabase, ISyncService syncService)
    {
        _apiService = apiService;
        _localDatabase = localDatabase;
        _syncService = syncService;
        
        _timer = Application.Current?.Dispatcher.CreateTimer() ?? throw new InvalidOperationException("No dispatcher available");
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
    }

    [RelayCommand]
    private async Task LoadSessionAsync(int sessionId)
    {
        IsLoading = true;
        
        try
        {
            CurrentSession = await _localDatabase.GetRoutineSessionAsync(sessionId);
            if (CurrentSession != null)
            {
                IsSessionActive = CurrentSession.IsRunning;
                await LoadActivitiesAsync();
                await LoadTemplatesAsync();
                
                if (IsSessionActive)
                {
                    _timer.Start();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading session: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        if (CurrentSession == null) return;

        CurrentSession.StartTime = DateTime.Now;
        CurrentSession.EndTime = null;
        CurrentSession.IsCompleted = false;
        
        IsSessionActive = true;
        _timer.Start();
        
        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
    }

    [RelayCommand]
    private async Task CompleteSessionAsync()
    {
        if (CurrentSession == null) return;

        CurrentSession.EndTime = DateTime.Now;
        CurrentSession.IsCompleted = true;
        
        IsSessionActive = false;
        _timer.Stop();
        
        // Stop any active activity
        if (ActiveActivity != null)
        {
            await CompleteActivityAsync(ActiveActivity);
        }
        
        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
    }

    [RelayCommand]
    private void StartAddingActivity()
    {
        IsAddingActivity = true;
        NewActivityName = string.Empty;
        SelectedTemplate = null;
    }

    [RelayCommand]
    private void CancelAddingActivity()
    {
        IsAddingActivity = false;
    }

    [RelayCommand]
    private async Task AddActivityAsync()
    {
        if (CurrentSession == null || string.IsNullOrWhiteSpace(NewActivityName))
            return;

        var newActivity = new SessionActivity
        {
            RoutineSessionId = CurrentSession.Id,
            RoutineSession = CurrentSession,
            ActivityTemplateId = SelectedTemplate?.Id,
            ActivityTemplate = SelectedTemplate,
            Name = NewActivityName,
            StartTime = DateTime.Now,
            IsCompleted = false,
            Order = Activities.Count + 1,
            Notes = string.Empty
        };

        // Add default metrics from template
        if (SelectedTemplate?.DefaultMetrics != null)
        {
            foreach (var templateMetric in SelectedTemplate.DefaultMetrics)
            {
                var metric = new ActivityMetric
                {
                    SessionActivityId = newActivity.Id,
                    SessionActivity = newActivity,
                    Name = templateMetric.Name,
                    Unit = templateMetric.Unit,
                    DataType = templateMetric.DataType,
                    DefaultValue = templateMetric.DefaultValue,
                    TextValue = templateMetric.DefaultValue
                };
                newActivity.Metrics.Add(metric);
            }
        }

        CurrentSession.Activities.Add(newActivity);
        Activities.Add(newActivity);
        
        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
        
        IsAddingActivity = false;
    }

    [RelayCommand]
    private async Task StartActivityAsync(SessionActivity activity)
    {
        if (activity == null) return;

        // Stop previous activity if any
        if (ActiveActivity != null && !ActiveActivity.IsCompleted)
        {
            await CompleteActivityAsync(ActiveActivity);
        }

        activity.StartTime = DateTime.Now;
        activity.EndTime = null;
        activity.IsCompleted = false;
        
        ActiveActivity = activity;
        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
    }

    [RelayCommand]
    private async Task CompleteActivityAsync(SessionActivity activity)
    {
        if (activity == null) return;

        activity.EndTime = DateTime.Now;
        activity.IsCompleted = true;
        
        if (ActiveActivity == activity)
        {
            ActiveActivity = null;
        }
        
        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
    }

    [RelayCommand]
    private async Task DeleteActivityAsync(SessionActivity activity)
    {
        if (activity == null || CurrentSession == null) return;

        CurrentSession.Activities.Remove(activity);
        Activities.Remove(activity);
        
        if (ActiveActivity == activity)
        {
            ActiveActivity = null;
        }
        
        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
    }

    public async Task UpdateActivityMetricAsync(ActivityMetric metric, string newValue)
    {
        if (metric == null) return;

        switch (metric.DataType)
        {
            case MetricDataType.Number:
                if (decimal.TryParse(newValue, out var numValue))
                {
                    metric.Value = numValue;
                    metric.TextValue = newValue;
                }
                break;
            case MetricDataType.Text:
            case MetricDataType.Boolean:
            default:
                metric.TextValue = newValue;
                break;
        }

        await _localDatabase.SaveRoutineSessionAsync(CurrentSession);
    }

    private async Task LoadActivitiesAsync()
    {
        if (CurrentSession == null) return;

        Activities.Clear();
        foreach (var activity in CurrentSession.Activities.OrderBy(a => a.Order))
        {
            Activities.Add(activity);
            
            // Check if this activity is currently active
            if (!activity.IsCompleted && activity.StartTime > DateTime.MinValue)
            {
                ActiveActivity = activity;
            }
        }
    }

    private async Task LoadTemplatesAsync()
    {
        var templates = await _apiService.GetActivityTemplatesAsync(CurrentSession?.HabitId);
        AvailableTemplates.Clear();
        foreach (var template in templates)
        {
            AvailableTemplates.Add(template);
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (CurrentSession != null)
        {
            var duration = CurrentSession.IsRunning ? 
                DateTime.Now.Subtract(CurrentSession.StartTime) : 
                CurrentSession.Duration;
            SessionDuration = duration.ToString(@"hh\:mm\:ss");
        }

        if (ActiveActivity != null && !ActiveActivity.IsCompleted)
        {
            var activityDurationTime = DateTime.Now.Subtract(ActiveActivity.StartTime);
            ActivityDuration = activityDurationTime.ToString(@"mm\:ss");
        }
        else
        {
            ActivityDuration = "00:00";
        }
    }
}