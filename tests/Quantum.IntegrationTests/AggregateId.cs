using System.Collections.Generic;

namespace Quantum.IntegrationTests;

public class AggregateId : IsAnIdentity<AggregateId>
{
    public string StreamId { get; }

    public AggregateId(string streamId)
    {
        StreamId = streamId;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return StreamId;
    }
}