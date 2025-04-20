using System;
using System.Collections.Generic;

namespace Quantum.EventSourcing.Projection;

public class CheckPoint : IsAValueObject<CheckPoint>
{
    public CheckPoint()
    {

    }


    public CheckPoint(string name, ulong commitPosition, ulong preparePosition, int version = 1)
    {
        Id = name;
        CommitPosition = commitPosition;
        PreparePosition = preparePosition;
        Version = version;
    }

    public string Id { get; set; }
    public ulong CommitPosition { get; set; }
    public ulong PreparePosition { get; set; }
    public int Version { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return CommitPosition;
        yield return PreparePosition;
        yield return Version;
    }
}