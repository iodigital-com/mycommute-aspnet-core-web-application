using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MyCommute.Data.Entities;
public class DataContext : DbContext
{
    private readonly string connectionString = string.Empty;
    
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Commute> Commutes => Set<Commute>();

    public DataContext() : base()
    {
        // this constructor is solely for dotnet ef migrations command
    }
    
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }
        
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // optionsBuilder.UseSqlServer(connectionString,
        //     x =>
        //     {
        //         x.UseNetTopologySuite();
        //     });
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(x => x.Name);
            entity
                .HasIndex(x => x.Email)
                .IsUnique();
        });
    }
    
    /// <summary>
    /// Automatically set CreatedAt and/or UpdatedAt fields on saving changes
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }
        return await base.SaveChangesAsync(true, cancellationToken);
    }
}