using System;
using Quantum.Core;

namespace Quantum.IntegrationTests.EventSourcing.TestSpecificClasses
{
    public class DummyDateTimeProvider : IDateTimeProvider
    {
        private DateTimeOffset _expectedToReturnDateTimeOffset;

        public void WhenCallUtcNowReturn(DateTimeOffset expectedToReturnDateTimeOffset) =>
            _expectedToReturnDateTimeOffset = expectedToReturnDateTimeOffset;

        public DateTimeOffset UtcDateTimeNow()
            => _expectedToReturnDateTimeOffset;

        public DateTimeOffset Yesterday()
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset Friday()
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset OneWeekFromNow()
        {
            throw new NotImplementedException();
        }

        public (short PersianYear, short PersianMonth) PersianYearMonth()
        {
            throw new NotImplementedException();
        }
    }
}