using HabitTrackerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerApp.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options)
    {
    }

    public DbSet<Habit> Habits { get; set; }
    public DbSet<DailyHabitEntry> DailyHabitEntries { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<HabitMetricDefinition> HabitMetricDefinitions { get; set; }
    public DbSet<DailyMetricValue> DailyMetricValues { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseLog> ExerciseLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // DailyMetricValues -> DailyHabitEntries
      modelBuilder.Entity<DailyMetricValue>()
        .HasOne(d => d.DailyHabitEntry)
        .WithMany(e => e.DailyMetricValues)
        .HasForeignKey(d => d.DailyHabitEntryId)
        .OnDelete(DeleteBehavior.Cascade); // or .Restrict / .NoAction

      // DailyMetricValues -> HabitMetricDefinitions
      modelBuilder.Entity<DailyMetricValue>()
        .HasOne(d => d.HabitMetricDefinition)
        .WithMany()
        .HasForeignKey(d => d.HabitMetricDefinitionId)
        .OnDelete(DeleteBehavior.Restrict); // <--- This typically fixes the problem

      modelBuilder.Entity<DailyMetricValue>()
        .Property(d => d.TextValue)
        .IsRequired(false); // or .HasColumnType("nvarchar(max)").IsRequired(false);

      // Exercise -> Habit
      modelBuilder.Entity<Exercise>()
        .HasOne(e => e.Habit)
        .WithMany(h => h.Exercises)
        .HasForeignKey(e => e.HabitId)
        .OnDelete(DeleteBehavior.Cascade);

      // ExerciseLog -> Exercise
      modelBuilder.Entity<ExerciseLog>()
        .HasOne(el => el.Exercise)
        .WithMany(e => e.ExerciseLogs)
        .HasForeignKey(el => el.ExerciseId)
        .OnDelete(DeleteBehavior.Cascade);

      // ExerciseLog -> DailyHabitEntry
      modelBuilder.Entity<ExerciseLog>()
        .HasOne(el => el.DailyHabitEntry)
        .WithMany()
        .HasForeignKey(el => el.DailyHabitEntryId)
        .OnDelete(DeleteBehavior.Cascade);

      // Indexes for performance
      modelBuilder.Entity<ExerciseLog>()
        .HasIndex(el => el.Date);

      modelBuilder.Entity<ExerciseLog>()
        .HasIndex(el => el.ExerciseId);

      modelBuilder.Entity<Exercise>()
        .HasIndex(e => e.HabitId);

      modelBuilder.Entity<Exercise>()
        .HasIndex(e => e.OrderIndex);
    }

  }

}
