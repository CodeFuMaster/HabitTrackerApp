using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;

namespace HabitTrackerApp.Core.Data;

/// <summary>
/// Enhanced EF Core DbContext for cross-platform SQLite database
/// Supports offline-first architecture with sync capabilities
/// </summary>
public class HabitTrackerDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Habit> Habits { get; set; }
    public DbSet<DailyHabitEntry> DailyHabitEntries { get; set; }
    
    // Enhanced tracking entities
    public DbSet<RoutineSession> RoutineSessions { get; set; }
    public DbSet<SessionActivity> SessionActivities { get; set; }
    public DbSet<ActivityTemplate> ActivityTemplates { get; set; }
    public DbSet<ActivityMetric> ActivityMetrics { get; set; }
    
    // Sync entities
    public DbSet<SyncRecord> SyncRecords { get; set; }
    public DbSet<DeviceInfo> DeviceInfos { get; set; }
    
    public HabitTrackerDbContext(DbContextOptions<HabitTrackerDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureBaseEntities(modelBuilder);
        ConfigureEnhancedEntities(modelBuilder);
        ConfigureSyncEntities(modelBuilder);
        ConfigureIndexes(modelBuilder);
    }
    
    private void ConfigureBaseEntities(ModelBuilder modelBuilder)
    {
        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
        
        // Habit configuration
        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Habits)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // DailyHabitEntry configuration
        modelBuilder.Entity<DailyHabitEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasOne(e => e.Habit)
                  .WithMany(h => h.DailyEntries)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.HabitId, e.Date }).IsUnique();
        });
    }
    
    private void ConfigureEnhancedEntities(ModelBuilder modelBuilder)
    {
        // RoutineSession configuration
        modelBuilder.Entity<RoutineSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasOne(e => e.Habit)
                  .WithMany(h => h.RoutineSessions)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // SessionActivity configuration
        modelBuilder.Entity<SessionActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasOne(e => e.RoutineSession)
                  .WithMany(rs => rs.Activities)
                  .HasForeignKey(e => e.RoutineSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.ActivityTemplate)
                  .WithMany()
                  .HasForeignKey(e => e.ActivityTemplateId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
        
        // ActivityTemplate configuration
        modelBuilder.Entity<ActivityTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasOne(e => e.Habit)
                  .WithMany(h => h.ActivityTemplates)
                  .HasForeignKey(e => e.HabitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ActivityMetric configuration
        modelBuilder.Entity<ActivityMetric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataType).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.TextValue).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Polymorphic relationship - can belong to either SessionActivity or ActivityTemplate
            entity.HasOne(e => e.SessionActivity)
                  .WithMany(sa => sa.Metrics)
                  .HasForeignKey(e => e.SessionActivityId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.ActivityTemplate)
                  .WithMany(at => at.DefaultMetrics)
                  .HasForeignKey(e => e.ActivityTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureSyncEntities(ModelBuilder modelBuilder)
    {
        // SyncRecord configuration
        modelBuilder.Entity<SyncRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RecordId).IsRequired();
            entity.Property(e => e.Operation).IsRequired();
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Timestamp).IsRequired();
            
            entity.HasIndex(e => new { e.TableName, e.RecordId, e.Timestamp });
            entity.HasIndex(e => e.DeviceId);
        });
        
        // DeviceInfo configuration
        modelBuilder.Entity<DeviceInfo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastSyncTime).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            entity.HasIndex(e => e.DeviceId).IsUnique();
        });
    }
    
    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Performance indexes for common queries
        modelBuilder.Entity<Habit>()
            .HasIndex(e => e.IsActive);
            
        modelBuilder.Entity<RoutineSession>()
            .HasIndex(e => new { e.HabitId, e.Date });
            
        modelBuilder.Entity<SessionActivity>()
            .HasIndex(e => new { e.RoutineSessionId, e.Order });
            
        modelBuilder.Entity<ActivityMetric>()
            .HasIndex(e => e.SessionActivityId);
    }
}