using System.Globalization; // for ISOWeek
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using System.Linq;
using System;

namespace HabitTrackerApp.Controllers
{
  public class StatisticsController : Controller
  {
    private readonly AppDbContext _context;

    public StatisticsController(AppDbContext context)
    {
      _context = context;
    }

    // GET: /Statistics/Statistics?habitId=1&weeksToShow=8&monthsToShow=6
    public async Task<IActionResult> Statistics(int habitId, int weeksToShow = 8, int monthsToShow = 6)
    {
      var habit = await _context.Habits
          .Include(h => h.DailyHabitEntries)
          .FirstOrDefaultAsync(h => h.Id == habitId);
      if (habit == null) return NotFound();

      var today = DateTime.Today;

      // 1) Exclude future-dated entries
      var validEntries = habit.DailyHabitEntries
          .Where(e => e.Date <= today)
          .OrderBy(e => e.Date)
          .ToList();

      // 2) Basic Stats
      int totalDaysLogged = validEntries.Count;
      int totalCompletions = validEntries.Count(e => e.IsCompleted);
      double completionRate = (totalDaysLogged == 0)
          ? 0
          : (totalCompletions * 100.0 / totalDaysLogged);

      // 3) Last 7 Days (today-6 through today)
      DateTime last7Start = today.AddDays(-6);
      var last7daysEntries = validEntries.Where(e => e.Date >= last7Start);
      int last7Completions = last7daysEntries.Count(e => e.IsCompleted);

      // 4) Current Week (Mon-Sun)
      DateTime mondayThisWeek = GetMonday(today);
      DateTime sundayThisWeek = mondayThisWeek.AddDays(6);
      int thisWeekCompletions = validEntries.Count(e => e.IsCompleted
          && e.Date >= mondayThisWeek
          && e.Date <= sundayThisWeek);

      // 5) Last Week
      DateTime mondayLastWeek = mondayThisWeek.AddDays(-7);
      DateTime sundayLastWeek = mondayLastWeek.AddDays(6);
      int lastWeekCompletions = validEntries.Count(e => e.IsCompleted
          && e.Date >= mondayLastWeek
          && e.Date <= sundayLastWeek);

      int trendDifference = thisWeekCompletions - lastWeekCompletions;
      // For a percentage: 
      // double trendPercent = (lastWeekCompletions == 0) ? 100 : 
      //                       (trendDifference * 100.0 / lastWeekCompletions);

      // 6) Streaks
      int currentStreak = CalculateCurrentStreak(validEntries);
      int longestStreak = CalculateLongestStreak(validEntries);

      // 7) Longest Gap
      int longestGap = CalculateLongestGap(validEntries);

      // 8) Days Since Last Miss (for daily habits):
      int daysSinceLastMiss = 0;
      if (habit.RecurrenceType == RecurrenceType.Daily)
      {
        daysSinceLastMiss = CalculateDaysSinceLastMiss(validEntries);
      }

      // 9) Average Score (if your DailyHabitEntry has a Score?)
      // Suppose Score is an int? (nullable). We'll only average non-null scores
      double averageScore = 0;
      var scoredEntries = validEntries.Where(e => e.Score.HasValue).ToList();
      if (scoredEntries.Any())
      {
        averageScore = scoredEntries.Average(e => e.Score.Value);
      }

      // 10) Weekly data for last N weeks
      // We'll define "last N weeks" from (today - (weeksToShow*7 -1)) to "today"
      DateTime earliestWeekDate = today.AddDays(-7 * (weeksToShow - 1));
      var weeklyQuery = validEntries
          .Where(e => e.Date >= earliestWeekDate);

      var completionsByWeek = weeklyQuery
          .GroupBy(e => new {
            e.Date.Year,
            WeekNum = ISOWeek.GetWeekOfYear(e.Date)
          })
          .Select(g => new WeeklyCompletionDto
          {
            Year = g.Key.Year,
            WeekNumber = g.Key.WeekNum,
            CompletionCount = g.Count(x => x.IsCompleted)
          })
          .OrderBy(x => x.Year).ThenBy(x => x.WeekNumber)
          .ToList();

      // 11) Monthly data for last N months
      // We'll define "earliestMonthDate" as the first day of (today minus N-1 months)
      // For example:
      DateTime earliestMonthStart = new DateTime(today.Year, today.Month, 1)
          .AddMonths(-(monthsToShow - 1));
      var monthlyQuery = validEntries
          .Where(e => e.Date >= earliestMonthStart);

      var completionsByMonth = monthlyQuery
          .GroupBy(e => new { e.Date.Year, e.Date.Month })
          .Select(g => new MonthlyCompletionDto
          {
            Year = g.Key.Year,
            Month = g.Key.Month,
            CompletionCount = g.Count(e => e.IsCompleted)
          })
          .OrderBy(x => x.Year).ThenBy(x => x.Month)
          .ToList();

      // 12) Prepare ViewModel
      var viewModel = new HabitStatisticsViewModel
      {
        HabitId = habit.Id,
        HabitName = habit.Name,

        TotalCompletions = totalCompletions,
        CompletionRate = completionRate,
        Last7DaysCompletions = last7Completions,
        CurrentStreak = currentStreak,
        LongestStreak = longestStreak,
        WeeklyCompletions = thisWeekCompletions,

        // new stuff
        TrendDifference = trendDifference,
        AverageScore = averageScore,
        DaysSinceLastMiss = daysSinceLastMiss,
        LongestGap = longestGap,

        WeeklyCompletionsList = completionsByWeek,
        MonthlyCompletions = completionsByMonth
      };

      // We'll pass the user’s chosen weeksToShow & monthsToShow back to the view
      ViewBag.WeeksToShow = weeksToShow;
      ViewBag.MonthsToShow = monthsToShow;

      return View(viewModel);
    }

    private DateTime GetMonday(DateTime input)
    {
      while (input.DayOfWeek != DayOfWeek.Monday)
      {
        input = input.AddDays(-1);
      }
      return input;
    }

    private int CalculateCurrentStreak(List<DailyHabitEntry> entries)
    {
      var today = DateTime.Today;
      var completedDates = entries
          .Where(e => e.IsCompleted)
          .Select(e => e.Date.Date)
          .Distinct()
          .OrderByDescending(d => d)
          .ToList();

      int streak = 0;
      var current = today;
      while (completedDates.Contains(current))
      {
        streak++;
        current = current.AddDays(-1);
      }
      return streak;
    }

    private int CalculateLongestStreak(List<DailyHabitEntry> entries)
    {
      var completedDates = entries
          .Where(e => e.IsCompleted)
          .Select(e => e.Date.Date)
          .Distinct()
          .OrderBy(d => d)
          .ToList();

      int longest = 0;
      int currentStreak = 0;
      DateTime? prev = null;
      foreach (var d in completedDates)
      {
        if (prev == null || (d - prev.Value).TotalDays == 1)
        {
          currentStreak++;
        }
        else
        {
          longest = Math.Max(longest, currentStreak);
          currentStreak = 1;
        }
        prev = d;
      }
      longest = Math.Max(longest, currentStreak);
      return longest;
    }

    // "Longest gap" = the biggest break in consecutive *completed* days
    //   i.e. max difference between consecutive completed days
    private int CalculateLongestGap(List<DailyHabitEntry> entries)
    {
      var completedDates = entries
          .Where(e => e.IsCompleted)
          .Select(e => e.Date.Date)
          .Distinct()
          .OrderBy(d => d)
          .ToList();

      int longestGap = 0;
      DateTime? prev = null;
      foreach (var d in completedDates)
      {
        if (prev != null)
        {
          int gap = (int)(d - prev.Value).TotalDays - 1;
          if (gap > longestGap)
            longestGap = gap;
        }
        prev = d;
      }
      return longestGap;
    }

    // For a "daily" habit: 
    //   # days since last day that was "missed."
    //   We'll define "missed" = a day up to 'today' that is not completed
    //   This might be simplistic if user doesn't have an entry for every day
    private int CalculateDaysSinceLastMiss(List<DailyHabitEntry> entries)
    {
      // If there are no entries, return 0 (or some default value)
      if (entries == null || !entries.Any())
      {
        return 0;
      }

      var earliest = entries.Min(e => e.Date);
      var today = DateTime.Today;

      var completedDates = entries
        .Where(e => e.IsCompleted)
        .Select(e => e.Date.Date)
        .Distinct()
        .ToHashSet();

      int count = 0;
      var current = today;
      while (current >= earliest)
      {
        if (!completedDates.Contains(current))
        {
          // Found a miss
          return count;
        }
        count++;
        current = current.AddDays(-1);
      }
      return count;
    }

  }
}
