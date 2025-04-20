using Quantum.UnitTests.Domain.HelperClasses;

namespace Quantum.UnitTests.Domain
{
    public class EntityTests
    {
        [Fact]
        public void TestEntityEquality()
        {
            FakeEntityIdentity isAnIdentity = new FakeEntityIdentity(1);

            IsAnEntity<FakeEntityIdentity> fakeEntity1 = new FakeEntity(isAnIdentity, "value1");
            IsAnEntity<FakeEntityIdentity> fakeEntity2 = new FakeEntity(isAnIdentity, "value2");

            Assert.Equal(fakeEntity1, fakeEntity2);
            Assert.True(fakeEntity1.Equals( fakeEntity2));
            Assert.True(fakeEntity1 == fakeEntity2);
        }

        [Fact]
        public void TestEntityEqualWithNull()
        {
            FakeEntityIdentity isAnIdentity = new FakeEntityIdentity(1);

            IsAnEntity<FakeEntityIdentity> fakeEntity1 = new FakeEntity(isAnIdentity, "value1");

            Assert.False(fakeEntity1.Equals(null));

            fakeEntity1.Should().NotBe(null);
        }
    }
}