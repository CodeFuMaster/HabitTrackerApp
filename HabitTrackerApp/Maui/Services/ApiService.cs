using System.Net.Http.Json;
using System.Text.Json;
using HabitTrackerMobile.Models;

namespace HabitTrackerMobile.Services;

/// <summary>
/// Implementation of API communication service
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("HabitTrackerAPI");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Habit operations
    public async Task<List<Habit>> GetHabitsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("habit");
            response.EnsureSuccessStatusCode();
            var habits = await response.Content.ReadFromJsonAsync<List<Habit>>(_jsonOptions);
            return habits ?? new List<Habit>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting habits: {ex.Message}");
            return new List<Habit>();
        }
    }

    public async Task<Habit?> GetHabitAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"habit/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Habit>(_jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting habit {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Habit> CreateHabitAsync(Habit habit)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("habit", habit, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Habit>(_jsonOptions) ?? habit;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating habit: {ex.Message}");
            throw;
        }
    }

    public async Task<Habit> UpdateHabitAsync(Habit habit)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"habit/{habit.Id}", habit, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Habit>(_jsonOptions) ?? habit;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating habit: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteHabitAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"habit/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting habit {id}: {ex.Message}");
            return false;
        }
    }

    // Daily entries
    public async Task<List<DailyHabitEntry>> GetDailyEntriesAsync(DateTime date)
    {
        try
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"habit/daily-entries/{dateStr}");
            response.EnsureSuccessStatusCode();
            var entries = await response.Content.ReadFromJsonAsync<List<DailyHabitEntry>>(_jsonOptions);
            return entries ?? new List<DailyHabitEntry>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting daily entries: {ex.Message}");
            return new List<DailyHabitEntry>();
        }
    }

    public async Task<DailyHabitEntry> CreateDailyEntryAsync(DailyHabitEntry entry)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("habit/daily-entry", entry, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DailyHabitEntry>(_jsonOptions) ?? entry;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating daily entry: {ex.Message}");
            throw;
        }
    }

    public async Task<DailyHabitEntry> UpdateDailyEntryAsync(DailyHabitEntry entry)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"habit/daily-entry/{entry.Id}", entry, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DailyHabitEntry>(_jsonOptions) ?? entry;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating daily entry: {ex.Message}");
            throw;
        }
    }

    // Routine sessions
    public async Task<List<RoutineSession>> GetRoutineSessionsAsync(int habitId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"enhanced/routine-sessions/{habitId}");
            response.EnsureSuccessStatusCode();
            var sessions = await response.Content.ReadFromJsonAsync<List<RoutineSession>>(_jsonOptions);
            return sessions ?? new List<RoutineSession>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting routine sessions: {ex.Message}");
            return new List<RoutineSession>();
        }
    }

    public async Task<RoutineSession?> GetRoutineSessionAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"enhanced/routine-session/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RoutineSession>(_jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting routine session {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<RoutineSession> CreateRoutineSessionAsync(RoutineSession session)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("enhanced/routine-session", session, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RoutineSession>(_jsonOptions) ?? session;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating routine session: {ex.Message}");
            throw;
        }
    }

    public async Task<RoutineSession> UpdateRoutineSessionAsync(RoutineSession session)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"enhanced/routine-session/{session.Id}", session, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RoutineSession>(_jsonOptions) ?? session;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating routine session: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteRoutineSessionAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"enhanced/routine-session/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting routine session {id}: {ex.Message}");
            return false;
        }
    }

    // Categories
    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("category");
            response.EnsureSuccessStatusCode();
            var categories = await response.Content.ReadFromJsonAsync<List<Category>>(_jsonOptions);
            return categories ?? new List<Category>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting categories: {ex.Message}");
            return new List<Category>();
        }
    }

    // Activity templates
    public async Task<List<ActivityTemplate>> GetActivityTemplatesAsync(int? habitId = null)
    {
        try
        {
            var endpoint = habitId.HasValue ? $"enhanced/activity-templates/{habitId}" : "enhanced/activity-templates";
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var templates = await response.Content.ReadFromJsonAsync<List<ActivityTemplate>>(_jsonOptions);
            return templates ?? new List<ActivityTemplate>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting activity templates: {ex.Message}");
            return new List<ActivityTemplate>();
        }
    }

    // Test endpoints
    public async Task<string> TestGymSessionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("enhanced/test-gym-session");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> TestMorningRoutineAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("enhanced/test-morning-routine");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}