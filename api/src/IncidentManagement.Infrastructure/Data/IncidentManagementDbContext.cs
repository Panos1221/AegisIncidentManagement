using Microsoft.EntityFrameworkCore;
using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Infrastructure.Data;

public class IncidentManagementDbContext : DbContext
{
    public IncidentManagementDbContext(DbContextOptions<IncidentManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agency> Agencies { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<Personnel> Personnel { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleTelemetry> VehicleTelemetry { get; set; }
    public DbSet<Incident> Incidents { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<IncidentLog> IncidentLogs { get; set; }
    public DbSet<ShiftTemplate> ShiftTemplates { get; set; }
    public DbSet<ShiftInstance> ShiftInstances { get; set; }
    public DbSet<ShiftAssignment> ShiftAssignments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<VehicleAssignment> VehicleAssignments { get; set; }
    public DbSet<FireStation> FireStations { get; set; }
    public DbSet<StationBoundary> StationBoundaries { get; set; }
    public DbSet<PatrolZone> PatrolZones { get; set; }
    public DbSet<PatrolZoneAssignment> PatrolZoneAssignments { get; set; }
    public DbSet<FireHydrant> FireHydrants { get; set; }
    public DbSet<CoastGuardStation> CoastGuardStations { get; set; }

    public DbSet<PoliceStation> PoliceStations { get; set; }
    
    public DbSet<Hospital> Hospitals { get; set; }
    
    public DbSet<Caller> Callers { get; set; }

    // New incident detail entities
    public DbSet<IncidentInvolvement> IncidentInvolvements { get; set; }
    public DbSet<IncidentCommander> IncidentCommanders { get; set; }
    public DbSet<IncidentFire> IncidentFires { get; set; }
    public DbSet<IncidentDamage> IncidentDamages { get; set; }
    public DbSet<Injury> Injuries { get; set; }
    public DbSet<Death> Deaths { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Agency configuration
        modelBuilder.Entity<Agency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.HasMany(e => e.Users).WithOne(e => e.Agency).HasForeignKey(e => e.AgencyId);
            entity.HasMany(e => e.Stations).WithOne(e => e.Agency).HasForeignKey(e => e.AgencyId);
            entity.HasMany(e => e.Incidents).WithOne(e => e.Agency).HasForeignKey(e => e.AgencyId);
        });

        // Station configuration
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Agency).WithMany(e => e.Stations).HasForeignKey(e => e.AgencyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Personnel).WithOne(e => e.Station).HasForeignKey(e => e.StationId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Vehicles).WithOne(e => e.Station).HasForeignKey(e => e.StationId).OnDelete(DeleteBehavior.NoAction);
        });

        // Personnel configuration
        modelBuilder.Entity<Personnel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Rank).HasMaxLength(100);
            entity.Property(e => e.BadgeNumber).HasMaxLength(20);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.VehicleAssignments).WithOne(e => e.Personnel).HasForeignKey(e => e.PersonnelId);
        });

        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Callsign).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.PlateNumber).HasMaxLength(20);
        });

        // VehicleTelemetry configuration
        modelBuilder.Entity<VehicleTelemetry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Vehicle).WithMany().HasForeignKey(e => e.VehicleId);
        });

        // Incident configuration
        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MainCategory).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SubCategory).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.Agency).WithMany(e => e.Incidents).HasForeignKey(e => e.AgencyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedByUserId);
            entity.HasOne(e => e.ClosedBy).WithMany().HasForeignKey(e => e.ClosedByUserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Assignments).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId);
            entity.HasMany(e => e.Logs).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId);
            entity.HasMany(e => e.Notifications).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId);
            entity.HasMany(e => e.Callers).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId);

            // New incident detail relationships
            entity.HasOne(e => e.Involvement).WithOne(e => e.Incident).HasForeignKey<IncidentInvolvement>(e => e.IncidentId);
            entity.HasMany(e => e.Commanders).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId);
            entity.HasMany(e => e.Injuries).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Deaths).WithOne(e => e.Incident).HasForeignKey(e => e.IncidentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Fire).WithOne(e => e.Incident).HasForeignKey<IncidentFire>(e => e.IncidentId);
            entity.HasOne(e => e.Damage).WithOne(e => e.Incident).HasForeignKey<IncidentDamage>(e => e.IncidentId);
        });

        // Assignment configuration
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        // IncidentLog configuration
        modelBuilder.Entity<IncidentLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.By).HasMaxLength(100);
        });

        // Caller configuration
        modelBuilder.Entity<Caller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.Incident).WithMany(e => e.Callers).HasForeignKey(e => e.IncidentId);
        });

        // ShiftTemplate configuration
        modelBuilder.Entity<ShiftTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RRule).HasMaxLength(500);
        });

        // ShiftInstance configuration
        modelBuilder.Entity<ShiftInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceTemplateName).HasMaxLength(100);
        });

        // ShiftAssignment configuration
        modelBuilder.Entity<ShiftAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ShiftInstance).WithMany().HasForeignKey(e => e.ShiftInstanceId);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SupabaseUserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.SupabaseUserId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Agency).WithMany(e => e.Users).HasForeignKey(e => e.AgencyId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Station).WithMany().HasForeignKey(e => e.StationId).OnDelete(DeleteBehavior.NoAction);
            entity.HasMany(e => e.Notifications).WithOne(e => e.User).HasForeignKey(e => e.UserId);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithMany(e => e.Notifications).HasForeignKey(e => e.IncidentId);
        });

        // VehicleAssignment configuration
        modelBuilder.Entity<VehicleAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Vehicle).WithMany().HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Personnel).WithMany(e => e.VehicleAssignments).HasForeignKey(e => e.PersonnelId).OnDelete(DeleteBehavior.Restrict);
        });

        // FireStation configuration
        modelBuilder.Entity<FireStation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.GeometryJson);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            entity.HasMany(e => e.Boundaries).WithOne(e => e.FireStation).HasForeignKey(e => e.FireStationId).OnDelete(DeleteBehavior.Cascade);
        });

        // StationBoundary configuration
        modelBuilder.Entity<StationBoundary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CoordinatesJson).IsRequired();
            entity.HasOne(e => e.FireStation).WithMany(e => e.Boundaries).HasForeignKey(e => e.FireStationId).OnDelete(DeleteBehavior.Cascade);
        });

        // PatrolZone configuration
        modelBuilder.Entity<PatrolZone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BoundaryCoordinates).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(7);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.Agency).WithMany().HasForeignKey(e => e.AgencyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Station).WithMany().HasForeignKey(e => e.StationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.VehicleAssignments).WithOne(e => e.PatrolZone).HasForeignKey(e => e.PatrolZoneId).OnDelete(DeleteBehavior.Cascade);
        });

        // PatrolZoneAssignment configuration
        modelBuilder.Entity<PatrolZoneAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssignedAt).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasOne(e => e.PatrolZone).WithMany(e => e.VehicleAssignments).HasForeignKey(e => e.PatrolZoneId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Vehicle).WithMany().HasForeignKey(e => e.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedByUser).WithMany().HasForeignKey(e => e.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.UnassignedByUser).WithMany().HasForeignKey(e => e.UnassignedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // FireHydrant configuration
        modelBuilder.Entity<FireHydrant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.AdditionalProperties).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.HasIndex(e => e.ExternalId).IsUnique();
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
        });

        // CoastGuardStation configuration
        modelBuilder.Entity<CoastGuardStation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameGr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Area).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Telephone).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
        });



        // PoliceStation configuration
        modelBuilder.Entity<PoliceStation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Gid).IsRequired();
            entity.Property(e => e.OriginalId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Sinoikia).HasMaxLength(100);
            entity.Property(e => e.Diam).HasMaxLength(100);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            entity.HasIndex(e => e.Gid);
        });

        // Hospital configuration
        modelBuilder.Entity<Hospital>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.AgencyCode).HasMaxLength(10);
            entity.Property(e => e.Latitude).IsRequired();
            entity.Property(e => e.Longitude).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            entity.HasIndex(e => e.AgencyCode);
        });

        // IncidentInvolvement configuration
        modelBuilder.Entity<IncidentInvolvement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OtherAgencies).HasMaxLength(1000);
            entity.Property(e => e.ServiceActions).HasMaxLength(1000);
            entity.Property(e => e.RescueInformation).HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithOne(e => e.Involvement).HasForeignKey<IncidentInvolvement>(e => e.IncidentId);
        });

        // IncidentCommander configuration
        modelBuilder.Entity<IncidentCommander>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Observations).HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithMany(e => e.Commanders).HasForeignKey(e => e.IncidentId);
            entity.HasOne(e => e.Personnel).WithMany().HasForeignKey(e => e.PersonnelId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.AssignedBy).WithMany().HasForeignKey(e => e.AssignedByUserId).OnDelete(DeleteBehavior.NoAction);
        });

        // Injury configuration
        modelBuilder.Entity<Injury>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithMany(e => e.Injuries).HasForeignKey(e => e.IncidentId).OnDelete(DeleteBehavior.Cascade);
        });

        // Death configuration
        modelBuilder.Entity<Death>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithMany(e => e.Deaths).HasForeignKey(e => e.IncidentId).OnDelete(DeleteBehavior.Cascade);
        });

        // IncidentFire configuration
        modelBuilder.Entity<IncidentFire>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BurnedArea).HasMaxLength(1000);
            entity.Property(e => e.BurnedItems).HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithOne(e => e.Fire).HasForeignKey<IncidentFire>(e => e.IncidentId);
        });

        // IncidentDamage configuration
        modelBuilder.Entity<IncidentDamage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OwnerName).HasMaxLength(200);
            entity.Property(e => e.TenantName).HasMaxLength(200);
            entity.Property(e => e.DamageAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SavedProperty).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IncidentCause).HasMaxLength(1000);
            entity.HasOne(e => e.Incident).WithOne(e => e.Damage).HasForeignKey<IncidentDamage>(e => e.IncidentId);
        });
    }
}