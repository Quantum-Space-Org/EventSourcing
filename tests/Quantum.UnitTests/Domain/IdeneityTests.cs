using Quantum.UnitTests.Domain.HelperClasses;
using Quantum.UnitTests.EventSourcing.CustomerAggregate;

namespace Quantum.UnitTests.Domain
{
    public class IdentityTests
    {
        [Fact]
        public void ToStringMethodTest()
        {
            var fakeEntityIdentity = new FakeEntityIdentity(1);
            fakeEntityIdentity.ToString()
                .Should()
                .BeEquivalentTo($"{nameof(FakeEntityIdentity)} - 1");

            var complexEntityId = new ComplexEntityId(1, "A");
            complexEntityId.ToString()
                .Should()
                .BeEquivalentTo($"{nameof(ComplexEntityId)} - 1 - A");
        }

        [Fact]
        public void CompareToNull()
        {
            FakeEntityIdentity nullIdentity = null;

            var result = nullIdentity != null;
            result.Should().BeFalse();
        }

        [Fact]
        public async Task compareNullAggregateRootWithNull()
        {
            Customer nullCustomer = null;

            (nullCustomer != null).Should().BeFalse();
            (null != nullCustomer).Should().BeFalse();
            (nullCustomer != nullCustomer).Should().BeFalse();

            (nullCustomer == null).Should().BeTrue();
            (null == nullCustomer).Should().BeTrue();
            (nullCustomer == nullCustomer).Should().BeTrue();
        }

        [Fact]
        public async Task compareNotNullAggregateRootWithNull()
        {
            var notNullCustomer = new Customer(new CustomerId("1"), new FullName("firt", "last"));

            (notNullCustomer != null).Should().BeTrue();
            (null != notNullCustomer).Should().BeTrue();

            (notNullCustomer == null).Should().BeFalse();
            (null == notNullCustomer).Should().BeFalse();

            (notNullCustomer != notNullCustomer).Should().BeFalse();
            (notNullCustomer == notNullCustomer).Should().BeTrue();
        }
    }
}