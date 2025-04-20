using System;
using FluentAssertions;
using Quantum.EventSourcing.Projection;

namespace Quantum.IntegrationTests.EventSourcing
{
    public class MySubscriber : ImAProjector
    {
        public static MySubscriber WhichIExpectThisEventReceivedTimes<T>(int times)
            => new MySubscriber
            {
                ExpectedEventType = typeof(T),
                Times = times
            };

        public int Times { get; set; }
        public int ActualTimes { get; set; }

        public Type ExpectedEventType { get; set; }
        public Type ActualEventType { get; set; }

        public override DbOperationCommand Transform(IsADomainEvent @event)
        {
            ActualEventType = @event.GetType();
            ActualTimes++;

            throw new Exception();
        }

        public void Verify()
        {
            ActualTimes.Should().Be(Times);
            ActualEventType.Should().Be(ExpectedEventType);
        }

    }
}