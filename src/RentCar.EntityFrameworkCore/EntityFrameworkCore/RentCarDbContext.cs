using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using RentCar.Vehicles;
using RentCar.Customers;
using RentCar.Rentals;
using RentCar.DamageReports;

namespace RentCar.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class RentCarDbContext :
    AbpDbContext<RentCarDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<DamageReport> DamageReports { get; set; }


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public RentCarDbContext(DbContextOptions<RentCarDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */
        builder.Entity<Vehicle>(b =>
{
    b.ToTable(RentCarConsts.DbTablePrefix + "Vehicles", RentCarConsts.DbSchema);
    b.ConfigureByConvention();
    b.Property(x => x.Brand).IsRequired().HasMaxLength(VehicleConsts.MaxBrandLength);
    b.Property(x => x.Model).IsRequired().HasMaxLength(VehicleConsts.MaxModelLength);
    b.Property(x => x.DailyPrice).HasPrecision(18, 2);
    b.Property(x => x.ExtraKmPrice).HasPrecision(18, 2);
    b.HasIndex(x => x.Status);
});
builder.Entity<CustomerProfile>(b =>
{
    b.ToTable(RentCarConsts.DbTablePrefix + "CustomerProfiles", RentCarConsts.DbSchema);
    b.ConfigureByConvention();
    b.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(CustomerProfileConsts.MaxLicenseNumberLength);
    b.Property(x => x.LicenseClass).IsRequired().HasMaxLength(CustomerProfileConsts.MaxLicenseClassLength);
    b.Property(x => x.Address).HasMaxLength(CustomerProfileConsts.MaxAddressLength);
    b.HasIndex(x => x.UserId).IsUnique();
});
builder.Entity<Rental>(b =>
{
    b.ToTable(RentCarConsts.DbTablePrefix + "Rentals", RentCarConsts.DbSchema);
    b.ConfigureByConvention();
    b.Property(x => x.DailyPrice).HasPrecision(18, 2);
    b.Property(x => x.ExtraKmPrice).HasPrecision(18, 2);
    b.Property(x => x.BasePrice).HasPrecision(18, 2);
    b.Property(x => x.ExtraKmFee).HasPrecision(18, 2);
    b.Property(x => x.LateFee).HasPrecision(18, 2);
    b.Property(x => x.TotalPrice).HasPrecision(18, 2);
    b.HasIndex(x => x.UserId);
    b.HasIndex(x => new { x.VehicleId, x.Status });
});
builder.Entity<DamageReport>(b =>
{
    b.ToTable(RentCarConsts.DbTablePrefix + "DamageReports", RentCarConsts.DbSchema);
    b.ConfigureByConvention();
    b.Property(x => x.Description).IsRequired().HasMaxLength(DamageReportConsts.MaxDescriptionLength);
    b.Property(x => x.DamageCost).HasPrecision(18, 2);
    b.HasIndex(x => x.UserId);
    b.HasIndex(x => x.RentalId);
}); 


        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(RentCarConsts.DbTablePrefix + "YourEntities", RentCarConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
