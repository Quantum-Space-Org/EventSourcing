using Quantum.DataBase;
using Quantum.EventSourcing.Projection;
using Quantum.EventSourcing.SqlServerProjector;

namespace Quantum.UnitTests.ProjectDomainEventTests;

public class PersonProjector : ImAProjector
{
    private readonly QuantumDbContext _quantumDbContext;

    public PersonProjector(QuantumDbContext quantumDbContext) => _quantumDbContext = quantumDbContext;

    public override DbOperationCommand Transform(IsADomainEvent @event)
        => On((dynamic)@event);

    public DbAddOperation<Person> On(PersonIsCreatedDomainEvent @event)
        => new DbAddOperation<Person>(_quantumDbContext, new Person())
            .Add(p =>
            {
                p.Id = int.Parse(@event.AggregateId);
                p.FirstName = @event.FirstName;
                p.LastName = @event.LastName;
            });

    public DbUpdateOperation<Person> On(PersonNameIsChangedDomainEvent @event)
        => new DbUpdateOperation<Person>(_quantumDbContext, p => p.Id == int.Parse(@event.AggregateId))
            .With(p =>
            {
                p.Id = int.Parse(@event.AggregateId);
                p.FirstName = @event.NewFirstName;
            });
}

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}