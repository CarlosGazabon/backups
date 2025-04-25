using Microsoft.EntityFrameworkCore;
using Inventio.Models;
using Inventio.Models.Views;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using inventio.Models.Views;
using inventio.Models.StoredProcedure;
using Inventio.Models.Notifications;

namespace Inventio.Data;
public class ApplicationDBContext : IdentityDbContext<User, Role, string>
{
    public ApplicationDBContext() { }
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
    {
    }

    public DbSet<FeatureFlags> FeatureFlags { get; set; } = default!;
    public DbSet<Hour> Hour { get; set; } = default!;

    public DbSet<Line> Line { get; set; }

    public DbSet<Productivity> Productivity { get; set; } = default!;

    public DbSet<ProductivityFlow> ProductivityFlow { get; set; } = default!;

    public DbSet<Shift> Shift { get; set; } = default!;

    public DbSet<Supervisor> Supervisor { get; set; } = default!;

    public DbSet<Product> Product { get; set; }

    public DbSet<DowntimeCategory> DowntimeCategory { get; set; } = default!;

    public DbSet<DowntimeSubCategory1> DowntimeSubCategory1 { get; set; } = default!;

    public DbSet<DowntimeSubCategory2> DowntimeSubCategory2 { get; set; } = default!;

    public DbSet<DowntimeCode> DowntimeCode { get; set; } = default!;

    public DbSet<DowntimeReason> DowntimeReason { get; set; } = default!;
    public DbSet<Summary> Summary { get; set; } = default!;
    public DbSet<ProductivitySummary> ProductivitySummary { get; set; } = default!;
    public DbSet<ProductionSummary> ProductionSummary { get; set; } = default!;
    public DbSet<RoleAccess> RoleAccess { get; set; } = default!;
    public DbSet<RoleRights> RoleRights { get; set; } = default!;
    public DbSet<ChangeOver> ChangeOver { get; set; } = default!;
    public DbSet<Icons> Icons { get; set; }
    public DbSet<Menu> Menu { get; set; }
    public DbSet<MenuRights> MenuRights { get; set; }
    public DbSet<Objective> Objective { get; set; }
    public DbSet<QualityIncidentReason> QualityIncidentReason { get; set; }
    public DbSet<QualityIncident> QualityIncident { get; set; }

    public DbSet<NotificationEventLog> NotificationEventLog { get; set; }
    public DbSet<NotificationInventory> NotificationInventory { get; set; }
    public DbSet<NotificationMessage> NotificationMessage { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplate { get; set; }

    //* VIEWS GOES HERE
    public virtual DbSet<VwDataEfficiency> VwDataEfficiency { get; set; } = default!;
    public virtual DbSet<VwDowntime> VwDowntime { get; set; } = default!;
    public virtual DbSet<VwUtilization> VwUtilization { get; set; } = default!;
    public virtual DbSet<ProductivityReport> ProductivityReport { get; set; } = default!;
    public virtual DbSet<VwPackageSku> VwPackageSku { get; set; } = default!;
    public virtual DbSet<VwDailySummary> VwDailySummary { get; set; } = default!;
    public virtual DbSet<VwGeneralEfficiency> VwGeneralEfficiency { get; set; }
    public virtual DbSet<VwSupervisorMetrics> VwSupervisorMetrics { get; set; } = default!;
    public virtual DbSet<VwDowntimeXSubCat> VwDowntimeXSubCat { get; set; }
    public virtual DbSet<VwCases> VwCases { get; set; } = default!;
    public virtual DbSet<VwDowntimeTrend> VwDowntimeTrend { get; set; }
    public virtual DbSet<VwDowntimePerSku> VwDowntimePerSku { get; set; } = default!;
    public virtual DbSet<VwStatisticalChangeOver> VwStatisticalChangeOver { get; set; }
    public virtual DbSet<VwProductivityWithDowntime> VwProductivityWithDowntime { get; set; }

    public virtual DbSet<GetEfficiency> GetEfficiency { get; set; } = default!;
    public virtual DbSet<SPGetDataEfficiency> GetDataEfficiency { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Line>(entity =>
        {
            entity.ToTable("Line");

            entity.Property(e => e.BottleScrap)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.CanScrap)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.Efficiency).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Scrap).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.PreformScrap)
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
            entity.Property(e => e.Utilization).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PouchScrap) // Configuración de la nueva columna
                .IsRequired()
                .HasDefaultValueSql("(CONVERT([bit],(0)))");
        });

        modelBuilder.Entity<Summary>().HasNoKey().ToView(nameof(Summary));
        modelBuilder.Entity<ProductivitySummary>().HasNoKey().ToView(nameof(ProductivitySummary));
        modelBuilder.Entity<ProductionSummary>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("ProductionSummary");

                entity.Property(e => e.ChangeHrs).HasColumnType("decimal(38, 6)");
                entity.Property(e => e.ChangeMins).HasColumnType("decimal(38, 2)");
                entity.Property(e => e.EffDen)
                    .HasColumnType("decimal(38, 6)")
                    .HasColumnName("EffDEN");
                entity.Property(e => e.EffDensku)
                    .HasColumnType("decimal(38, 6)")
                    .HasColumnName("EffDENSKU");
                entity.Property(e => e.Efficiency).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.NetHrs).HasColumnType("decimal(38, 6)");
                entity.Property(e => e.Production).HasColumnType("decimal(38, 2)");
                entity.Property(e => e.Sku).HasColumnName("SKU");
            });


        modelBuilder.Entity<ChangeOver>(entity =>
              {
                  entity.ToTable("ChangeOver");
                  entity.Property(e => e.HourSort).HasColumnName("Hour_Sort");
                  entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
              });

        modelBuilder.Entity<VwDataEfficiency>(entity =>
        {
            entity.HasNoKey().ToView("vwDataEfficiency");
            entity.Property(p => p.Production).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Minutes).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<VwDowntime>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwDowntime");

            entity.Property(e => e.HoraSort)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("Hora_Sort");
            entity.Property(e => e.Hours)
                .HasColumnType("decimal(37, 21)")
                .HasColumnName("hours");
            entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExtraMinutes).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Shift).HasColumnName("shift_");
            entity.Property(e => e.Sku).HasColumnName("SKU");
        });

        modelBuilder.Entity<VwUtilization>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwUtilization");

            entity.Property(e => e.ChangeHrs).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.NetHrs).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.NetPlantHrs).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.Sku).HasColumnName("SKU");
        });

        modelBuilder.Entity<ProductivityReport>(e =>
        {
            e.HasNoKey().ToView("vwProductivityReport");
            e.Property(p => p.Downtime_minutes).HasColumnType("decimal(18,2)");
        });


        modelBuilder.Entity<VwPackageSku>(e =>
        {
            e.HasNoKey().ToView("vwPackageSku");
            e.Property(p => p.Production).HasColumnType("decimal(18,2)");
            e.Property(p => p.DownHrs).HasColumnType("decimal(18,2)");
            e.Property(p => p.MaxProduction).HasColumnType("decimal(18,2)");
            e.Property(p => p.NetHrs).HasColumnType("decimal(18,2)");
        });

        // modelBuilder.Entity<VwDailySummary>(e =>
        // {
        //     e.HasNoKey();
        //     e.Property(p => p.Production).HasColumnType("decimal(18,2)");
        // });

        modelBuilder.Entity<Icons>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Icons__3214EC071FEBF8E9");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Menu__3213E83F75A31B61");

            entity.ToTable("Menu");

            entity.Property(e => e.Id).HasMaxLength(255);
            entity.Property(e => e.Label).HasMaxLength(255);
            entity.Property(e => e.ParentId)
                .HasMaxLength(255)
                .HasColumnName("Parent_id");
            entity.Property(e => e.Route).HasMaxLength(255);
        });

        modelBuilder.Entity<MenuRights>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MenuRigh__3213E83FB1DA2D1C");

            entity.Property(e => e.CanAdd)
                .HasDefaultValueSql("((0))")
                .HasColumnName("Can_add");
            entity.Property(e => e.CanDelete)
                .HasDefaultValueSql("((0))")
                .HasColumnName("Can_delete");
            entity.Property(e => e.CanModify)
                .HasDefaultValueSql("((0))")
                .HasColumnName("Can_modify");
            entity.Property(e => e.CanView)
                .HasDefaultValueSql("((0))")
                .HasColumnName("Can_view");
            entity.Property(e => e.MenuId)
                .HasMaxLength(255)
                .HasColumnName("Menu_id");
            entity.Property(e => e.RoleId)
                .HasMaxLength(450)
                .HasColumnName("Role_id");
        });

        //* ----------------------------- Register views ----------------------------- */

        modelBuilder.Entity<VwDailySummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VwDailySummary");

            entity.Property(p => p.Production).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<VwGeneralEfficiency>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwGeneralEfficiency");

            entity.Property(e => e.ChangeHrs).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.ChangeMins).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.EffDen)
                .HasColumnType("decimal(38, 6)")
                .HasColumnName("EffDEN");
            entity.Property(e => e.NetHrs).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.Production).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<VwSupervisorMetrics>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwSupervisorMetrics");
            entity.Property(e => e.Production).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.ScrapDen).HasColumnType("decimal(38, 2)");
        });


        modelBuilder.Entity<VwCases>(entity =>
        {
            entity.HasNoKey()
            .ToView("VwCases");

            entity.Property(p => p.Production).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<VwDowntimePerSku>(entity =>
        {
            entity.HasNoKey()
            .ToView("VwDowntimePerSku");

            entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExtraMinutes).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<VwDowntimeXSubCat>(entity =>
       {
           entity
               .HasNoKey()
               .ToView("vwDowntimeXSubCat");

           entity.Property(e => e.Hours)
               .HasColumnType("decimal(37, 21)")
               .HasColumnName("hours");
           entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
           entity.Property(e => e.ExtraMinutes).HasColumnType("decimal(18, 2)");
           entity.Property(e => e.Shift).HasColumnName("shift");
       });

        modelBuilder.Entity<VwDowntimeTrend>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwDowntimeTrend");

            entity.Property(e => e.Hours)
                .HasColumnType("decimal(37, 21)")
                .HasColumnName("hours");
            entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExtraMinutes).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<VwStatisticalChangeOver>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwStatisticalChangeOver");

            entity.Property(e => e.Hours)
                .HasColumnType("decimal(37, 21)")
                .HasColumnName("hours");
            entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Shift).HasColumnName("shift");
            entity.Property(e => e.ShiftId).HasColumnName("shiftId");
        });

        modelBuilder.Entity<VwProductivityWithDowntime>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vwProductivityWithDowntime");

            entity.Property(e => e.LineId).HasColumnName("LineID");
            entity.Property(e => e.Minutes).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Sku).HasColumnName("SKU");
        });

        //* ---------------------------- Stored Procedure ---------------------------- */
        modelBuilder.Entity<GetEfficiency>(entity =>
        {
            entity.HasNoKey().ToView(null);
            entity.Property(p => p.DownHrs).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Production).HasColumnType("decimal(18,2)");
            entity.Property(p => p.MaxProduction).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<SPGetDataEfficiency>(entity =>
        {
            entity.HasNoKey().ToView(null);
            entity.Property(p => p.DownHrs).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Production).HasColumnType("decimal(18,2)");
            entity.Property(p => p.MaxProduction).HasColumnType("decimal(18,2)");
        });
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Productivity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            // ((Productivity)entityEntry.Entity).UpdatedDate = DateTime.Now;

            if (entityEntry.State == EntityState.Added)
            {
                ((Productivity)entityEntry.Entity).CreatedDate = DateTime.Now;
            }
        }

        return base.SaveChanges();
    }

}