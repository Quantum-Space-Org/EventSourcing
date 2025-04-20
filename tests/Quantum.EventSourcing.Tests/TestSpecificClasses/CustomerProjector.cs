using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Quantum.EventSourcing.Projection;
using Quantum.EventSourcing.SqlServerProjector;

namespace Quantum.EventSourcing.Tests.TestSpecificClasses
{

    public class CustomerProjector : ImAProjector
    {
        private readonly QuantumDbContext _dbContext;

        public CustomerProjector(QuantumDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override DbOperationCommand Transform(IsADomainEvent @event)
        {
            return On((dynamic)@event);
        }

        private DbAddOperation<Customer> On(ANewCustomerIsCreatedEvent @event)
        {
            return new DbAddOperation<Customer>(_dbContext, new Customer())
                .Add(c =>
                {
                    c.Id = @event.CustomerId;
                    c.FirstName = @event.FirstName;
                    c.LastName = @event.LastName;
                });
        }

    }

}

public class Customer
{
    [Key]
    public string Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class CustomerDbContext : QuantumDbContext
{
    public DbSet<Customer> Customers { get; set; }
    public CustomerDbContext()
        : base(new DbContextConfig(QuantumDbContextOptions4Sqlite))
    {

        Database.EnsureCreated();
    }

    private static DbContextOptionsBuilder<QuantumDbContext> QuantumDbContextOptions4Sqlite =>
        new DbContextOptionsBuilder<QuantumDbContext>()
            .UseSqlite(CreateInMemoryDatabase());

    private static DbConnection CreateInMemoryDatabase()
    {
        var connection = new SqliteConnection("Filename=:memory:");

        connection.Open();

        return connection;
    }
}