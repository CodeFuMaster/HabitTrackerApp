using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;
using HabitTrackerApp.Core.Services.Sync;

namespace HabitTrackerApp.Core.Data
{
    public class HabitTrackerDbContext : DbContext
    {
        // Core entities
        public DbSet<Habit> Habits { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<DailyHabitEntry> DailyHabitEntries { get; set; }
        public DbSet<HabitMetricDefinition> HabitMetricDefinitions { get; set; }
        public DbSet<DailyMetricValue> DailyMetricValues { get; set; }

        // Enhanced entities for complex routine tracking
        public DbSet<RoutineSession> RoutineSessions { get; set; }
        public DbSet<SessionActivity> SessionActivities { get; set; }
        public DbSet<ActivityTemplate> ActivityTemplates { get; set; }
        public DbSet<ActivityMetric> ActivityMetrics { get; set; }
        public DbSet<ActivityMetricTemplate> ActivityMetricTemplates { get; set; }

        // Sync-related entities
        public DbSet<SyncRecord> SyncLogs { get; set; }
        public DbSet<AppSetting> Settings { get; set; }

        public HabitTrackerDbContext(DbContextOptions<HabitTrackerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureCoreEntities(modelBuilder);
            ConfigureEnhancedEntities(modelBuilder);
            ConfigureSyncEntities(modelBuilder);
            ConfigureIndexes(modelBuilder);
        }

        private void ConfigureCoreEntities(ModelBuilder modelBuilder)
        {
            // Habit configuration
            modelBuilder.Entity<Habit>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Name).IsRequired().HasMaxLength(200);
                entity.Property(h => h.Description).HasMaxLength(1000);
                entity.Property(h => h.ShortDescription).HasMaxLength(500);
                entity.Property(h => h.Tags).HasMaxLength(500);
                entity.Property(h => h.ImageUrl).HasMaxLength(500);
                entity.Property(h => h.WeeklyDays).HasMaxLength(100);
                entity.Property(h => h.MonthlyDays).HasMaxLength(100);
                entity.Property(h => h.DeviceId).HasMaxLength(50);

                entity.HasOne(h => h.Category)
                      .WithMany(c => c.Habits)
                      .HasForeignKey(h => h.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(h => h.DailyHabitEntries)
                      .WithOne(e => e.Habit)
                      .HasForeignKey(e => e.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(h => h.RoutineSessions)
                      .WithOne(rs => rs.Habit)
                      .HasForeignKey(rs => rs.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.Property(c => c.ImageUrl).HasMaxLength(255);
                entity.Property(c => c.DeviceId).HasMaxLength(50);
            });

            // DailyHabitEntry configuration
            modelBuilder.Entity<DailyHabitEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Reflection).HasMaxLength(2000);
                entity.Property(e => e.DeviceId).HasMaxLength(50);

                entity.HasOne(e => e.Habit)
                      .WithMany(h => h.DailyHabitEntries)
                      .HasForeignKey(e => e.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.DailyMetricValues)
                      .WithOne(v => v.DailyHabitEntry)
                      .HasForeignKey(v => v.DailyHabitEntryId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent duplicate entries for same habit/date
                entity.HasIndex(e => new { e.HabitId, e.Date }).IsUnique();
            });

            // DailyMetricValue configuration
            modelBuilder.Entity<DailyMetricValue>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.TextValue).HasMaxLength(1000);
                entity.Property(v => v.DeviceId).HasMaxLength(50);

                entity.HasOne(v => v.DailyHabitEntry)
                      .WithMany(e => e.DailyMetricValues)
                      .HasForeignKey(v => v.DailyHabitEntryId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.HabitMetricDefinition)
                      .WithMany()
                      .HasForeignKey(v => v.HabitMetricDefinitionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // HabitMetricDefinition configuration
            modelBuilder.Entity<HabitMetricDefinition>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Unit).HasMaxLength(50);
                entity.Property(m => m.DeviceId).HasMaxLength(50);

                entity.HasOne(m => m.Habit)
                      .WithMany(h => h.MetricDefinitions)
                      .HasForeignKey(m => m.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureEnhancedEntities(ModelBuilder modelBuilder)
        {
            // RoutineSession configuration
            modelBuilder.Entity<RoutineSession>(entity =>
            {
                entity.HasKey(rs => rs.Id);
                entity.Property(rs => rs.Notes).HasMaxLength(2000);
                entity.Property(rs => rs.DeviceId).HasMaxLength(50);

                entity.HasOne(rs => rs.Habit)
                      .WithMany(h => h.RoutineSessions)
                      .HasForeignKey(rs => rs.HabitId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(rs => rs.Activities)
                      .WithOne(sa => sa.RoutineSession)
                      .HasForeignKey(sa => sa.RoutineSessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent duplicate sessions for same habit/date
                entity.HasIndex(rs => new { rs.HabitId, rs.Date }).IsUnique();
            });

            // SessionActivity configuration
            modelBuilder.Entity<SessionActivity>(entity =>
            {
                entity.HasKey(sa => sa.Id);
                entity.Property(sa => sa.Name).IsRequired().HasMaxLength(200);
                entity.Property(sa => sa.Notes).HasMaxLength(1000);
                entity.Property(sa => sa.DeviceId).HasMaxLength(50);

                entity.HasOne(sa => sa.RoutineSession)
                      .WithMany(rs => rs.Activities)
                      .HasForeignKey(sa => sa.RoutineSessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sa => sa.ActivityTemplate)
                      .WithMany()
                      .HasForeignKey(sa => sa.ActivityTemplateId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(sa => sa.Metrics)
                      .WithOne(am => am.SessionActivity)
                      .HasForeignKey(am => am.SessionActivityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ActivityTemplate configuration
            modelBuilder.Entity<ActivityTemplate>(entity =>
            {
                entity.HasKey(at => at.Id);
                entity.Property(at => at.Name).IsRequired().HasMaxLength(200);
                entity.Property(at => at.Description).HasMaxLength(1000);
                entity.Property(at => at.Instructions).HasMaxLength(2000);
                entity.Property(at => at.Tags).HasMaxLength(500);
                entity.Property(at => at.DeviceId).HasMaxLength(50);

                entity.HasMany(at => at.DefaultMetrics)
                      .WithOne(amt => amt.ActivityTemplate)
                      .HasForeignKey(amt => amt.ActivityTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ActivityMetric configuration
            modelBuilder.Entity<ActivityMetric>(entity =>
            {
                entity.HasKey(am => am.Id);
                entity.Property(am => am.Name).IsRequired().HasMaxLength(100);
                entity.Property(am => am.TextValue).HasMaxLength(1000);
                entity.Property(am => am.Unit).HasMaxLength(20);
                entity.Property(am => am.DeviceId).HasMaxLength(50);

                entity.HasOne(am => am.SessionActivity)
                      .WithMany(sa => sa.Metrics)
                      .HasForeignKey(am => am.SessionActivityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(am => am.Template)
                      .WithMany()
                      .HasForeignKey(am => am.ActivityMetricTemplateId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ActivityMetricTemplate configuration
            modelBuilder.Entity<ActivityMetricTemplate>(entity =>
            {
                entity.HasKey(amt => amt.Id);
                entity.Property(amt => amt.Name).IsRequired().HasMaxLength(100);
                entity.Property(amt => amt.Unit).HasMaxLength(20);
                entity.Property(amt => amt.DefaultValue).HasMaxLength(100);
                entity.Property(amt => amt.HelpText).HasMaxLength(500);
                entity.Property(amt => amt.DeviceId).HasMaxLength(50);

                entity.HasOne(amt => amt.ActivityTemplate)
                      .WithMany(at => at.DefaultMetrics)
                      .HasForeignKey(amt => amt.ActivityTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureSyncEntities(ModelBuilder modelBuilder)
        {
            // SyncRecord configuration (renamed from SyncLog to avoid conflicts)
            modelBuilder.Entity<SyncRecord>(entity =>
            {
                entity.HasKey(sr => sr.Id);
                entity.Property(sr => sr.TableName).IsRequired().HasMaxLength(100);
                entity.Property(sr => sr.Operation).IsRequired().HasMaxLength(20);
                entity.Property(sr => sr.Data).IsRequired();
                entity.Property(sr => sr.DeviceId).IsRequired().HasMaxLength(50);

                entity.ToTable("SyncLogs"); // Map to SyncLogs table
            });

            // AppSetting configuration
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.HasKey(s => s.Key);
                entity.Property(s => s.Key).HasMaxLength(100);
                entity.Property(s => s.Value).HasMaxLength(2000);

                entity.ToTable("Settings");
            });
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Performance indexes for common queries
            modelBuilder.Entity<Habit>()
                .HasIndex(h => h.IsDeleted);

            modelBuilder.Entity<Habit>()
                .HasIndex(h => h.CategoryId);

            modelBuilder.Entity<DailyHabitEntry>()
                .HasIndex(e => e.Date);

            modelBuilder.Entity<DailyHabitEntry>()
                .HasIndex(e => e.IsCompleted);

            modelBuilder.Entity<RoutineSession>()
                .HasIndex(rs => rs.Date);

            modelBuilder.Entity<RoutineSession>()
                .HasIndex(rs => rs.IsCompleted);

            modelBuilder.Entity<SessionActivity>()
                .HasIndex(sa => sa.Type);

            modelBuilder.Entity<SessionActivity>()
                .HasIndex(sa => sa.IsCompleted);

            // Sync-related indexes for performance
            modelBuilder.Entity<SyncRecord>()
                .HasIndex(sr => new { sr.Synced, sr.Timestamp });

            modelBuilder.Entity<SyncRecord>()
                .HasIndex(sr => sr.DeviceId);

            // Add DeviceId indexes for sync performance
            modelBuilder.Entity<Habit>()
                .HasIndex(h => h.DeviceId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.DeviceId);

            modelBuilder.Entity<DailyHabitEntry>()
                .HasIndex(e => e.DeviceId);
        }
    }

    // Helper entities for sync and settings are defined in Services.Sync namespace

    public class AppSetting
    {
        public required string Key { get; set; }
        public string? Value { get; set; }
    }
}