using System;
using System.Collections.Generic;
using System.Reflection;
using Quantum.EventSourcing.Projection;

namespace Quantum.EventSourcing;

public class Ledger: ILedger
{
    private readonly Assembly _assembly;
    private readonly Dictionary<Type, List<Type>> _result;
    public Ledger(Assembly assembly)
    {
        _assembly = assembly;
        _result = new Dictionary<Type, List<Type>>();
        Resolve();
    }

    internal  virtual void Resolve()
    {

        List<Type> projectorTypes = _assembly.ResolveChildrenOf(typeof(ImAProjector));

        foreach (var projectorType in projectorTypes)
        {
            List<Type> types = projectorType.InterestIn();

            foreach (var type in types)
            {
                if (_result.TryGetValue(type, out List<Type> projectors))
                {
                    projectors.Add(projectorType);
                }
                else
                    _result[type] = new List<Type> { projectorType };
            }
        }
    }

    public List<Type> WhoAreInterestedIn(Type type)
    {
        if (_result.TryGetValue(type, out List<Type> result))
            return result;

        return EmptyLitOfTypes();
    }

    private List<Type> EmptyLitOfTypes() => new List<Type>();
}