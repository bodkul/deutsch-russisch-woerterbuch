using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Woerterbuch.Core.Entities;

namespace Woerterbuch.Core;

public class WoerterbuchDbContext(DbContextOptions<WoerterbuchDbContext> options) : DbContext(options)
{
    public DbSet<DictEntry> DictEntries => Set<DictEntry>();
    public DbSet<Word> Words => Set<Word>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DictEntry>(e =>
        {
            e.Property(x => x.De).HasMaxLength(200);
            e.Property(x => x.Gender).HasMaxLength(3);
            e.Property(x => x.Pos).HasMaxLength(50);
            e.OwnsMany(x => x.Senses, s => s.ToJson());
            e.HasIndex(x => x.DeNorm);
        });

        modelBuilder.Entity<Word>(e =>
        {
            e.Property(x => x.De).HasMaxLength(200);
            e.Property(x => x.Article).HasMaxLength(3);
            e.Property(x => x.Plural).HasMaxLength(200);
            e.Property(x => x.Pos).HasMaxLength(50);
            e.HasOne(x => x.DictEntry).WithMany().OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        BeforeSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        BeforeSave();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void BeforeSave()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            if (entry.Entity is INormalizable n)
                n.Normalize();

            if (entry.Entity is Word w)
            {
                if (entry.State == EntityState.Added)
                    w.CreatedAt = now;
                w.UpdatedAt = now;
            }
        }
    }
}

/// <summary>Required by <c>dotnet ef</c>.</summary>
public class WoerterbuchDbContextFactory : IDesignTimeDbContextFactory<WoerterbuchDbContext>
{
    public WoerterbuchDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<WoerterbuchDbContext>()
            .UseSqlite(WoerterbuchDb.DefaultConnectionString())
            .Options);
}
