using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Quantum.Core;
using Quantum.DataBase;
using Quantum.EventSourcing.Tests.TestSpecificClasses;
using Quantum.Resolver;


namespace Quantum.EventSourcing.Tests.DomainEventApplier
{
    public class DomainEventApplierShould
    {
        private readonly QuantumDbContext NullDbbContext = NullDbContext.Instance;
        [Fact]
        public async Task append_domain_events_to_ES_Db_and_assert_event_store()
        {
            IEventStore eventStore = new InMemoryEventStore.InMemoryEventStore(new DateTimeProvider());
            var mockProjector = new MockProjector();

            var domainEventApplier = DomainEventApplier(StubedResolver.WhichReturn(mockProjector),
                NullDbbContext, 
                StubLedger.WhichReturn(typeof(MockProjector)),
                eventStore);

            await domainEventApplier.ApplyEventToAndProject(new
                FakeEntityIdentity(1), new ANewCustomerIsCreatedEvent("1", "Martin", "Fowler"));

            var events = await eventStore.LoadEventStreamAsync(new FakeEntityIdentity(1));
            events.Count.Should().Be(1);
            mockProjector.Verify();
        }


        [Fact]
        public async Task append_domain_events_to_ES_Db_and_assert_dbContext()
        {
            var customerDbContext = new CustomerDbContext();

            var domainEventApplier = DomainEventApplier(StubedResolver.WhichReturn(new CustomerProjector(customerDbContext)),
                customerDbContext, StubLedger.WhichReturn(typeof(CustomerProjector)), new InMemoryEventStore.InMemoryEventStore(new DateTimeProvider()));
            
            await domainEventApplier.ApplyEventToAndProject(new
                FakeEntityIdentity(1), new ANewCustomerIsCreatedEvent("1", "Martin", "Fowler"));

            var customer = await customerDbContext.Customers.FirstOrDefaultAsync();

            customer.Should().NotBeNull();
            customer.FirstName.Should().BeEquivalentTo("Martin");
            customer.LastName.Should().BeEquivalentTo("Fowler");
        }

        private static Test.DomainEventApplier DomainEventApplier(IResolver resolver, 
            QuantumDbContext quantumDbContext, 
            ILedger ledger, IEventStore inMemoryEventStore)
        {
            return new Test.DomainEventApplier(
                ledger,
                inMemoryEventStore,
                resolver,
                quantumDbContext);
        }
        
    }
}