using System.Threading.Tasks;

namespace Quantum.EventSourcing.Projection;

public interface IDeDuplicator
{
    Task<bool> IsThisADuplicateEventWeHaveSeenBefore(string eventId, string eventType);
    Task Save(string id, string eventType);
}