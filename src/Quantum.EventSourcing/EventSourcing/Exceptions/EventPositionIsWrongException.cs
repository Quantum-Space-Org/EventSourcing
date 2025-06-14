﻿using System;

namespace Quantum.EventSourcing.Exceptions;

public class EventPositionIsWrongException : Exception
{
    public EventPositionIsWrongException(string eventStreamId, EventStreamPosition eventStreamPosition)
    {
        EventStreamId = eventStreamId;
        Position = eventStreamPosition;
    }

    public string EventStreamId { get; }
    public EventStreamPosition Position { get; }
}