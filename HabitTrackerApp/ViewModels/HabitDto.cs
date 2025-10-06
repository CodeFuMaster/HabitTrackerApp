using System;
using HabitTrackerApp.Models;

namespace HabitTrackerApp.ViewModels;

public class HabitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public HabitType HabitType { get; set; } = HabitType.Simple;
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Daily;
    public int? RecurrenceInterval { get; set; }
    public string? WeeklyDays { get; set; }
    public string? MonthlyDays { get; set; }
    public int[]? SpecificDaysOfWeek { get; set; }
    public int[]? SpecificDaysOfMonth { get; set; }
    public DateTime? SpecificDate { get; set; }
    public string? TimeOfDay { get; set; }
    public string? TimeOfDayEnd { get; set; }
    public int? DurationMinutes { get; set; }
    public int? CategoryId { get; set; }
    public string? Tags { get; set; }
    public string? ImageUrl { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool ReminderEnabled { get; set; }
    public string? ReminderTime { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? LastModifiedDate { get; set; }
    public string? DeviceId { get; set; }
    public string? SyncStatus { get; set; }
}
