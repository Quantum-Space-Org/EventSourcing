using System.Threading.Tasks;

namespace Quantum.EventSourcing.Projection;

public class NullDeDuplicator : IDeDuplicator
{
    public static IDeDuplicator Instance => new NullDeDuplicator();
    public async Task<bool> IsThisADuplicateEventWeHaveSeenBefore(string eventId, string eventType) => false;
    public Task Save(string id, string eventType)
    {
        return Task.CompletedTask;
    }
}