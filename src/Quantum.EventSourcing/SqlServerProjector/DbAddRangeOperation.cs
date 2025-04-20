using System;
using System.Threading.Tasks;
using Quantum.DataBase;
using Quantum.DataBase.EntityFramework;
using Quantum.EventSourcing.Projection;

namespace Quantum.EventSourcing.SqlServerProjector;

public class DbAddRangeOperation<T> : DbOperationCommand
    where T : class
{
    private static Action<T> _action;
    private readonly QuantumDbContext _quantumDbContext;
    private readonly T _initialState;

    public DbAddRangeOperation(QuantumDbContext quantumDbContext, T initialState)
    {
        _quantumDbContext = quantumDbContext;
        _initialState = initialState;
    }

    public DbAddRangeOperation<T> Add(Action<T> action)
    {
        _action = action;
        return this;
    }

    public override async Task Execute()
    {
        _action.Invoke(_initialState);

        var dbSet = _quantumDbContext.Get<T>();
        await dbSet.AddRangeAsync(_initialState);
        //await _quantumDbContext.SaveChangesAsync();
    }
}