using Microsoft.EntityFrameworkCore;

public class NullDbContext(DbContextOptionsBuilder<QuantumDbContext> options)
    : QuantumDbContext(new DbContextConfig(options))
{
    public static QuantumDbContext Instance => new NullDbContext(
        new DbContextOptionsBuilder<QuantumDbContext>()
            .UseSqlite()
        );
}