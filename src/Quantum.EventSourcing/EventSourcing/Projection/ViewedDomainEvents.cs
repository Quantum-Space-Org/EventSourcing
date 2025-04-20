using System;
using System.ComponentModel.DataAnnotations;

namespace Quantum.EventSourcing.Projection;

public class ViewedDomainEvents
{
    [Key]
    public string EventId { get; set; }
    public string EventType { get; set; }
    public bool Successful { get; set; }

    public DateTime SeenAt { get; set; }

    public ViewedDomainEvents()
    {
            
    }

    public ViewedDomainEvents(string eventId, string eventType, bool successful)
    {
        EventId = eventId;
        EventType = eventType;
        Successful = successful;
        SeenAt = DateTime.Now;
    }
}