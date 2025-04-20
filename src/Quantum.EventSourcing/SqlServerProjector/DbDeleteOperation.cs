using Microsoft.EntityFrameworkCore;
using Quantum.DataBase;
using Quantum.EventSourcing.Projection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Quantum.DataBase.EntityFramework;

namespace Quantum.EventSourcing.SqlServerProjector
{
    public class DbDeleteOperation<T> : DbOperationCommand
        where T : class
    {
        private QuantumDbContext _quantumDbContext;
        private Expression<Func<T, bool>> _prediction;

        public DbDeleteOperation(QuantumDbContext quantumDbContext, Expression<Func<T, bool>> prediction)
        {
            _quantumDbContext = quantumDbContext;
            _prediction = prediction;
        }

        public override async Task Execute()
        {
            var dbSet = _quantumDbContext.Get<T>();
            var entity = await dbSet.Where(_prediction).ToListAsync();

            if (entity == null || !entity.Any())
                return;

            dbSet.RemoveRange(entity);

            //await _quantumDbContext.SaveChangesAsync();
            //_quantumDbContext.ChangeTracker.Clear();
        }
    }
}