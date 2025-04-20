using Quantum.EventSourcing.Projection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Quantum.EventSourcing;

public interface IDocumentStore
{
    Task<T> Fetch<T>(Expression<Func<T, bool>> findExpression)
        where T : class;
    Task<List<T>> FetchAll<T>(Expression<Func<T, bool>> findExpression)
        where T : class;
    Task<List<T>> FetchAll<T>()
        where T : class;
    Task StoreCheckpoint(CheckPoint checkpoint);
    Task<CheckPoint> GetCheckPoint(string name);
    Task SaveEventAsViewed(string eventCorrelationId, string eventType, bool successful);
    Task<ViewedDomainEvents> GetEventAsViewed(string eventCorrelationId, string eventType);
}