using HabitTrackerApp.Core.Data;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerApp.MAUI.Services;

public class HabitService : IHabitService
{
    private readonly HabitTrackerDbContext _context;

    public HabitService(HabitTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<Habit>> GetAllHabitsAsync()
    {
        return await _context.Habits
            .Include(h => h.Category)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Habit>> GetHabitsAsync()
    {
        return await GetAllHabitsAsync();
    }

    public async Task<List<Habit>> GetHabitsForDateAsync(DateTime date)
    {
        return await _context.Habits
            .Include(h => h.Category)
            .Where(h => !h.IsDeleted)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Habit?> GetHabitByIdAsync(int id)
    {
        return await _context.Habits
            .Include(h => h.Category)
            .Include(h => h.RoutineSessions)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Habit> CreateHabitAsync(Habit habit)
    {
        _context.Habits.Add(habit);
        await _context.SaveChangesAsync();
        return habit;
    }

    public async Task<Habit> UpdateHabitAsync(Habit habit)
    {
        _context.Habits.Update(habit);
        await _context.SaveChangesAsync();
        return habit;
    }

    public async Task DeleteHabitAsync(int id)
    {
        var habit = await _context.Habits.FindAsync(id);
        if (habit != null)
        {
            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<RoutineSession?> GetTodaysSessionAsync(int habitId)
    {
        var today = DateTime.Today;
        return await _context.RoutineSessions
            .Include(rs => rs.Activities)
            .ThenInclude(a => a.Metrics)
            .FirstOrDefaultAsync(rs => rs.HabitId == habitId && rs.Date.Date == today);
    }

    public async Task<RoutineSession> StartRoutineSessionAsync(int habitId)
    {
        var session = new RoutineSession
        {
            HabitId = habitId,
            Date = DateTime.Today,
            StartedAt = DateTime.Now,
            IsCompleted = false,
            Activities = new List<SessionActivity>()
        };

        _context.RoutineSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task CompleteRoutineSessionAsync(int sessionId)
    {
        var session = await _context.RoutineSessions.FindAsync(sessionId);
        if (session != null)
        {
            session.IsCompleted = true;
            session.CompletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<SessionActivity> AddSessionActivityAsync(SessionActivity activity)
    {
        _context.SessionActivities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }

    public async Task UpdateSessionActivityAsync(SessionActivity activity)
    {
        _context.SessionActivities.Update(activity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSessionActivityAsync(int activityId)
    {
        var activity = await _context.SessionActivities.FindAsync(activityId);
        if (activity != null)
        {
            _context.SessionActivities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }

    // Additional session management methods
    public async Task<RoutineSession?> GetActiveSessionAsync(int habitId)
    {
        return await _context.RoutineSessions
            .Include(rs => rs.Activities)
            .FirstOrDefaultAsync(rs => rs.HabitId == habitId && !rs.IsCompleted);
    }

    public async Task<RoutineSession> StartSessionAsync(RoutineSession session)
    {
        _context.RoutineSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task CompleteSessionAsync(int sessionId)
    {
        await CompleteRoutineSessionAsync(sessionId);
    }

    public async Task<List<RoutineSession>> GetCompletedSessionsAsync(int habitId, DateTime date)
    {
        return await _context.RoutineSessions
            .Where(rs => rs.HabitId == habitId && 
                        rs.Date.Date == date.Date && 
                        rs.IsCompleted)
            .ToListAsync();
    }

    public async Task<List<RoutineSession>> GetRecentSessionsAsync(int habitId, int count)
    {
        return await _context.RoutineSessions
            .Where(rs => rs.HabitId == habitId)
            .OrderByDescending(rs => rs.StartedAt)
            .Take(count)
            .ToListAsync();
    }

    // Category management methods
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}