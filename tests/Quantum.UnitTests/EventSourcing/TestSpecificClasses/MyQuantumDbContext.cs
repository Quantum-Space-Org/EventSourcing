using Microsoft.EntityFrameworkCore;
using Quantum.DataBase;
using Quantum.EventSourcing.Projection;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Projector;

namespace Quantum.UnitTests.EventSourcing.TestSpecificClasses
{
    public class MyQuantumDbContext : QuantumDbContext
    {
        public MyQuantumDbContext(DbContextOptionsBuilder<QuantumDbContext> options)
            : base(new DbContextConfig(options))
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            Database.Migrate();
        }

        public DbSet<CustomerViewModel> CustomerViewModels { get; set; }
        public DbSet<CheckPoint> CheckPoints { get; set; }
    }
}