using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;

namespace Quantum.UnitTests.EventSourcing.CustomerAggregate
{
    public class Customer : IsAnAggregateRoot<CustomerId>
    {
        public FullName FullName { get; private set; }

        public Customer(params IsADomainEvent[] events)
        : base(default)
        {
            foreach (var isADomainEvent in events)
            {

                if (isADomainEvent.GetType() == typeof(ANewCustomerIsCreatedEvent))
                {
                    Identity =new CustomerId( ((ANewCustomerIsCreatedEvent)isADomainEvent).AggregateId);
                    FullName = new FullName(((ANewCustomerIsCreatedEvent)isADomainEvent).FirstName
                         , ((ANewCustomerIsCreatedEvent)isADomainEvent).LastName);
                }
                else if (isADomainEvent.GetType() == typeof(CustomerNameIsChangedEvent))
                {
                    Identity = new CustomerId(((CustomerNameIsChangedEvent)isADomainEvent).AggregateId);
                    FullName = new FullName(((CustomerNameIsChangedEvent)isADomainEvent).FirstName
                        , ((CustomerNameIsChangedEvent)isADomainEvent).LastName);
                }

            }
        }
        public Customer(CustomerId identity, FullName fullName) : base(identity)
        {

            FullName = fullName;

            var @event = new ANewCustomerIsCreatedEvent(Identity.Id
                , FullName.FirstName, FullName.LastName);

            Apply(@event);
        }

        public void ChangeName(FullName fullName)
        {
            FullName = fullName;

            var @event = new CustomerNameIsChangedEvent(Identity.Id
                , FullName.FirstName, FullName.LastName);

            Apply(@event);
        }

        internal void Delete()
        {
            Apply(new CustomerIsDeletedEvent(Identity.Id));
        }
    }
}