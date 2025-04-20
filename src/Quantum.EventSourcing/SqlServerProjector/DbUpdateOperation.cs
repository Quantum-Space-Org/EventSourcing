using Quantum.DataBase.EntityFramework;

namespace Quantum.EventSourcing.SqlServerProjector
{
    using DataBase;
    using Microsoft.EntityFrameworkCore;
    using Projection;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class DbUpdateOperation<T> : DbOperationCommand
        where T : class
    {
        private readonly QuantumDbContext _quantumDbContext;

        private Action<T> _action { get; set; }

        private Expression<Func<T, bool>> _prediction { get; }

        public DbUpdateOperation(QuantumDbContext quantumDbContext, Expression<Func<T, bool>> prediction)
        {
            _quantumDbContext = quantumDbContext;
            _prediction = prediction;
        }


        public DbUpdateOperation<T> With(Action<T> action)
        {
            _action = action;
            return this;
        }

        public override async Task Execute()
        {
            var dbSet = _quantumDbContext.Get<T>();

            var entity = await dbSet.FirstOrDefaultAsync(_prediction);

            if (entity is null)
            {
                entity = GetEntityFromChangeTracker();

                if (entity == null) throw new EntityNotFoundException(typeof(T));

                _action.Invoke(entity);
            }
            else
            {
                _action.Invoke(entity);
                dbSet.Update(entity);
            }

            //LogEntityStateBeforeUpdate(entity);



            //LogEntityStateAfterUpdate(entity);

            //await _quantumDbContext.SaveChangesAsync();
        }

        private T GetEntityFromChangeTracker()
        {
            if (_quantumDbContext.ChangeTracker.Entries().Any() is false) return null;

            var entityEntries = _quantumDbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is T)
                .ToArray();

            if (entityEntries.Any() is false) return null;

            return entityEntries.Select(e => (T)e.Entity)
                .SingleOrDefault(_prediction.Compile());
        }

        private void LogEntityStateBeforeUpdate(T entity)
        {
            LogInfo("DbUpdateOperation Before apply update command : {@entity}", entity);
            LogTrackedEntries("Before apply update command");
        }

        private void LogEntityStateAfterUpdate(T entity)
        {
            LogInfo("DbUpdateOperation After apply update command : {@entity}", entity);
            LogTrackedEntries("After apply update command");
        }

        private void LogTrackedEntries(string message)
        {
            var trackedEntries = _quantumDbContext.ChangeTracker.Entries();
            LogInfo("DbUpdateOperation {message}, tracked entries are : {@trackedEntries}", trackedEntries.Select(t => new
            {
                t.State,
                t.Entity,
                t.CurrentValues,
                t.OriginalValues,
                t.Navigations
            }));
        }

        private void LogInfo(string message, object entity)
        {
            Serilog.Log.Logger.Information(message, entity);
        }
    }
}