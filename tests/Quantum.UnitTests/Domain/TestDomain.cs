using Quantum.EventSourcing.Test;
using Quantum.UnitTests.EventSourcing.CustomerAggregate;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;
using Xunit.Sdk;

namespace Quantum.UnitTests.Domain
{
    public class TestDomain
    {
        readonly CustomerId customerId = new("1");
        readonly FullName martinFowler = new("Martin", "Fowler");
        readonly FullName gregYoung = new("Greg", "Young");

        [Fact]
        public void TestCreatingANewAggregate()
        {
            InTermsOfAggregateRoot<Customer, CustomerId>
                .IfICreate(() => new Customer(customerId, gregYoung))
                .ThenIWillExpectTheseEvents(new ANewCustomerIsCreatedEvent(customerId.Id, gregYoung.FirstName, gregYoung.LastName))
                .And(a => a.FullName.FirstName == "Greg" && a.FullName.LastName == "Young");
        }


        [Fact]
        public void TestCreatingANewAggregate_CheckStateByAListOfActions()
        {
            InTermsOfAggregateRoot<Customer, CustomerId>
                .IfICreate(() => new Customer(customerId, gregYoung))
                .ThenIWillExpectTheseEvents(new ANewCustomerIsCreatedEvent(customerId.Id, gregYoung.FirstName, gregYoung.LastName))
                .And(a => a.FullName.FirstName.Should().BeEquivalentTo("Greg"), a => a.FullName.LastName.Should().BeEquivalentTo("Young")); ;
        }

        [Fact]
        public void PropertyTest()
        {
            InTermsOfAggregateRoot<Customer, CustomerId>
                .IfICreate(() => new Customer(customerId, gregYoung))
                .ThenIWillExpect(a => a.FullName.FirstName.Should().BeEquivalentTo("Greg"),
                    a => a.FullName.LastName.Should().BeEquivalentTo("Young"));
        }
        
        [Fact]
        public void TestCreatingANewAggregate_CheckStateByAListOfActions_OneAssertionIsFailed()
        {
            var gregg = "Gregg";

            var a = InTermsOfAggregateRoot<Customer, CustomerId>
                .IfICreate(() => new Customer(customerId, gregYoung));

            var action = () => a.And(a => a.FullName.FirstName.Should().BeEquivalentTo(gregg), a => a.FullName.LastName.Should().BeEquivalentTo("Young"));
            action.Should().Throw<XunitException>();
        }

        [Fact]
        public void TestReconstituteAnAggregateAndTriggerSomeBehavior()
        {
            InTermsOfAggregateRoot<Customer, CustomerId>
               .IfIApplied(new ANewCustomerIsCreatedEvent(customerId.Id, gregYoung.FirstName, gregYoung.LastName))
               .WhenICall(aggr => aggr.ChangeName(martinFowler))
               .ThenIWillExpectTheseEvents(new CustomerNameIsChangedEvent(customerId.Id, martinFowler.FirstName, martinFowler.LastName))
               .And(a => a.FullName.FirstName == "Martin" && a.FullName.LastName == "Fowler");
        }
    }
}