using Microsoft.EntityFrameworkCore;
using Quantum.DataBase;
using Quantum.EventSourcing.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Quantum.DataBase.EntityFramework;

namespace Quantum.EventSourcing.SqlServerDocumentStore
{
    public class SqlServerDocumentStore : IDocumentStore
    {
        private readonly QuantumDbContext _quantumDbContext;

        public SqlServerDocumentStore(QuantumDbContext quantumDbContext) => _quantumDbContext = quantumDbContext;


        public async Task<T> Fetch<T>(Expression<Func<T, bool>> findExpression) where T : class
        {
            var dbEntity = _quantumDbContext.Get<T>();
            var entity = dbEntity.FirstOrDefault(findExpression);
            return entity == null ? default : entity as T;
        }

        public async Task<List<T>> FetchAll<T>(Expression<Func<T, bool>> findExpression) where T : class
        {
            var dbEntity = _quantumDbContext.Get<T>();
            var entities = await dbEntity
                .Where(findExpression)
                .ToListAsync();
            return entities;
        }

        
        public async Task<List<T>> FetchAll<T>() where T : class
        {
            var dbEntity = _quantumDbContext.Get<T>();
            var entities = await dbEntity.ToListAsync();
            return entities;
        }

        public async Task StoreCheckpoint(CheckPoint checkpoint)
        {
            var dbEntity = _quantumDbContext.Get<CheckPoint>();
            var entity = dbEntity.FirstOrDefault(c => c.Id == checkpoint.Id);

            if (entity == null)
            {
                await dbEntity.AddAsync(checkpoint);
            }
            else
            {
                entity.CommitPosition = checkpoint.CommitPosition;
                entity.PreparePosition = checkpoint.PreparePosition;
                entity.Version = checkpoint.Version;
            }
        }

        public async Task<CheckPoint> GetCheckPoint(string name)
        {
            var dbEntity = _quantumDbContext.Get<CheckPoint>();
            var entity = await dbEntity.FirstOrDefaultAsync(c => c.Id == name);
            return entity;
        }

        public async Task SaveEventAsViewed(string eventCorrelationId, string eventType, bool successful)
        {
            var dbEntity = _quantumDbContext.Get<ViewedDomainEvents>();
            var entity = dbEntity.FirstOrDefault(c => c.EventId == eventCorrelationId);

            if (entity == null)
            {
                await dbEntity.AddAsync(new ViewedDomainEvents(eventId: eventCorrelationId, eventType: eventType, successful: successful));
            }
            else
            {
                entity.EventType = eventType;
                entity.Successful = successful;
            }
        }

        public async Task<ViewedDomainEvents> GetEventAsViewed(string eventCorrelationId, string eventType)
        {
            var dbEntity = _quantumDbContext.Get<ViewedDomainEvents>();
            var entity = await dbEntity.FirstOrDefaultAsync(c => c.EventId == eventCorrelationId);
            return entity;
        }
    }
}