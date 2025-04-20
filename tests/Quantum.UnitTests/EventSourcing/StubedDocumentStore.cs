using System.Linq.Expressions;
using Quantum.EventSourcing;
using Quantum.EventSourcing.Projection;

namespace Quantum.UnitTests.EventSourcing
{
    public class StubedDocumentStore : IDocumentStore
    {
        private readonly Dictionary<string, string> _dictionary = new();

        public Task<T> Fetch<T>(Expression<Func<T, bool>> findExpression) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> FetchAll<T>(Expression<Func<T, bool>> findExpression) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> FetchAll<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public Task StoreCheckpoint(CheckPoint checkpoint)
        {
            throw new NotImplementedException("StoreCheckpoint");
        }

        public Task<CheckPoint> GetCheckPoint(string name)
        {
            throw new NotImplementedException();
        }

        public Task SaveEventAsViewed(string eventCorrelationId, string eventType, bool successful)
        {
            _dictionary[eventCorrelationId] = eventType;
            return Task.CompletedTask;
        }

        public async Task<ViewedDomainEvents> GetEventAsViewed(string eventCorrelationId, string eventType)
        {
            ViewedDomainEvents result = null;
            if (_dictionary.TryGetValue(eventCorrelationId, out var _))
            {
                result = new ViewedDomainEvents(eventCorrelationId, eventType, true);
            }

            return result;
        }
    }
}