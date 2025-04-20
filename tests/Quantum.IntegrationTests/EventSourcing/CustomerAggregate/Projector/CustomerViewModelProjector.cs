using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;
using Quantum.EventSourcing;
using Quantum.EventSourcing.Projection;
using Quantum.EventSourcing.SqlServerProjector;
using Quantum.IntegrationTests.EventSourcing.CustomerAggregate.Events;

namespace Quantum.IntegrationTests.EventSourcing.CustomerAggregate.Projector
{
    public class CustomerViewModelProjector : ImAProjector
    {
        private bool _isCalled;
        private readonly List<IsADomainEvent> _receivedEvents = new List<IsADomainEvent>();

        private readonly QuantumDbContext _quantumDbContext;
        public CustomerViewModelProjector(IDocumentStore documentStore, QuantumDbContext quantumDbContext)
        {
            _quantumDbContext = quantumDbContext;
        }

        public override DbOperationCommand Transform( IsADomainEvent @event)
        {
            _receivedEvents.Add(@event);

            var dbOperation = On((dynamic)@event);
            _isCalled = true;
            return dbOperation;
        }


        public DbAddOperation<CustomerViewModel> On( ANewCustomerIsCreatedEvent customerNameIsChangedEvent)
        {
            return new DbAddOperation<CustomerViewModel>(_quantumDbContext,new CustomerViewModel()).Add(s =>
               {
                   s.FirstName = customerNameIsChangedEvent.FirstName;
                   s.LastName = customerNameIsChangedEvent.LastName;
                   s.Id = customerNameIsChangedEvent.CustomerId;
               });
        }

        public DbUpdateOperation<CustomerViewModel> On( CustomerNameIsChangedEvent customerNameIsChangedEvent)
        {
            return new DbUpdateOperation<CustomerViewModel>(_quantumDbContext, c => c.Id == customerNameIsChangedEvent.AggregateId).With(s =>
                {
                    s.FirstName = customerNameIsChangedEvent.FirstName;
                    s.LastName = customerNameIsChangedEvent.LastName;
                });
        }

        public DbDeleteOperation<CustomerViewModel> On(CustomerIsDeletedEvent customerIsDeletedEvent)
        {
            return new DbDeleteOperation<CustomerViewModel>(_quantumDbContext, c => c.Id == customerIsDeletedEvent.AggregateId);
        }

        public void Verify()
            => _isCalled.Should().BeTrue();

        public bool IsCalled()
            => _isCalled;

        public bool IsCalledWith<TEvent>() => _receivedEvents.Any(t => t.GetType() == typeof(TEvent));
        internal void VerifyThatItIsReceivesJustCustomerNameIsChangedEvent(CustomerNameIsChangedEvent nameIsChangedEvent)
        {
            _receivedEvents.Count().Should().Be(1);
            _receivedEvents.Single().GetId().Should().Be(nameIsChangedEvent.GetId());
        }

        internal void ResetEvents() 
            => _receivedEvents.Clear();
    }
}