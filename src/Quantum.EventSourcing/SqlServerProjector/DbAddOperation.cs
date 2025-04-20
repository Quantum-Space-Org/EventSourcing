using Quantum.DataBase.EntityFramework;

namespace Quantum.EventSourcing.SqlServerProjector;

using System;
using System.Threading.Tasks;
using DataBase;
using Projection;

public class DbAddOperation<T> : DbOperationCommand
    where T : class
{
    private static Action<T> _action;
    private readonly QuantumDbContext _quantumDbContext;
    private readonly T _initialState;

    public DbAddOperation(QuantumDbContext quantumDbContext, T initialState)
    {
        _quantumDbContext = quantumDbContext;
        _initialState = initialState;
    }

    public DbAddOperation<T> Add(Action<T> action)
    {
        _action = action;
        return this;
    }

    public override async Task Execute()
    {
        _action.Invoke(_initialState);

        var dbSet = _quantumDbContext.Get<T>();
        await dbSet.AddAsync(_initialState);
        //await _quantumDbContext.SaveChangesAsync();
    }
}