using Quantum.EventSourcing.Projection;
using Quantum.UnitTests.EventSourcing;

namespace Quantum.UnitTests
{
    public class MockProjector : ImAProjector
        {
            private bool _isCalled;

            public override DbOperationCommand Transform(IsADomainEvent @event)
            {
                _isCalled = true;
                return NullDbOperationCommand.Instance;
            }
            public void Verify()
            {
                _isCalled.Should().BeTrue();
            }
        }
    
}
