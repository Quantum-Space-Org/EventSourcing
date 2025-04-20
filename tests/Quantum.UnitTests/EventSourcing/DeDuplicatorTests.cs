using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Quantum.EventSourcing;
using Quantum.EventSourcing.Subscriber;
using Quantum.UnitTests.EventSourcing.CustomerAggregate.Events;
using Quantum.UnitTests.EventSourcing.TestSpecificClasses;

namespace Quantum.UnitTests.EventSourcing
{
    public class DeDuplicatorTests
    {
        [Fact]
        public async Task DeDuplicator()
        {
            //Given 
            var mySubscriber = MyProjector.WhichIExpectThisEventReceivedTimes<ANewCustomerIsCreatedEvent>(2);

            ILedger ledger = new StubedLedger();
            var resolver = StubedResolver.WhichResolve(mySubscriber);
            ILogger<NotifyProjectorsSubscriber> logger = NullLogger<NotifyProjectorsSubscriber>.Instance;
            
            ICatchUpSubscriber subscriber = new NotifyProjectorsSubscriber(ledger, resolver, logger, new MyQuantumDbContext(null));

            var eventViewModel = new EventViewModel
            {
                Version = 1,
                Payload = new ANewCustomerIsCreatedEvent("1", "Greg", "Young"),
                EventId = Guid.NewGuid().ToString(),
                EventType = typeof(ANewCustomerIsCreatedEvent).FullName,
            };

            subscriber.AnEventAppended(eventViewModel);

            //Again attempt to project event again
            subscriber.AnEventAppended(eventViewModel);

            mySubscriber.Verify();
        }
    }
}