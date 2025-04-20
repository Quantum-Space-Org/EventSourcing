using System.Threading.Tasks;
using Quantum.Domain;

namespace Quantum.EventSourcing.Versioning;

public interface IEventStoreVerioner
{
    Task CopyAndReplaceEventStream(IsAnIdentity from, IsAnIdentity to, bool deleteOldStream = false);
    Task CopyTransformAndReplaceEventStream(IsAnIdentity from, IsAnIdentity to, bool deleteOldStream);
}