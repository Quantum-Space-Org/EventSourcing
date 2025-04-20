using System;
using System.Collections.Generic;

namespace Quantum.EventSourcing;

public abstract class ViewModelBase : IsAValueObject<ViewModelBase>
{
    public DateTime CreatedAt { get; set; }
    public ViewModelBase()
    {
        CreatedAt = DateTime.Now;

    }
    public abstract override IEnumerable<object> GetEqualityComponents();
}