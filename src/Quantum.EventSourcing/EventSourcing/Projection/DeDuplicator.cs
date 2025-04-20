using System.Threading.Tasks;

namespace Quantum.EventSourcing.Projection;

public class DeDuplicator : IDeDuplicator
{
    private readonly IDocumentStore _documentStore;

    public DeDuplicator(IDocumentStore documentStore) => _documentStore = documentStore;

    public async Task<bool> IsThisADuplicateEventWeHaveSeenBefore(string eventId, string eventType)
    {
        var viewedDomainEvents = await _documentStore.GetEventAsViewed(eventId, eventType);

        return viewedDomainEvents != null;
    }

    public async Task Save(string id, string eventType)
        => await _documentStore.SaveEventAsViewed(id, eventType, successful: true);
}