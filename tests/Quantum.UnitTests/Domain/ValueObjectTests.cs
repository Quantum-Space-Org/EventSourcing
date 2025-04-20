using Quantum.UnitTests.Domain.HelperClasses;

namespace Quantum.UnitTests.Domain
{
    public class ValueObjectTests
    {
        [Fact]
        public void TestValueObjectEquality()
        {
            IsAValueObject<FakeValueObject> fakeValueObject1 = new FakeValueObject("GetValue");
            IsAValueObject<FakeValueObject> fakeValueObject2 = new FakeValueObject("GetValue");

            Assert.Equal(fakeValueObject1, fakeValueObject2);
            Assert.True(fakeValueObject1.Equals(fakeValueObject2));
            Assert.True(fakeValueObject1==fakeValueObject2);
        }

        [Fact]
        public void TestValueObjectNotEquality()
        {
            IsAValueObject<FakeValueObject> fakeValueObject1 = new FakeValueObject("Value1");
            IsAValueObject<FakeValueObject> fakeValueObject2 = new FakeValueObject("Value2");

            Assert.NotEqual(fakeValueObject1, fakeValueObject2);
            Assert.False(fakeValueObject1.Equals( fakeValueObject2));
            Assert.False(fakeValueObject1==fakeValueObject2);
        }


    }

}