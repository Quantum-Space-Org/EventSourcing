﻿namespace Quantum.EventSourcing;

public enum EventStreamPositions
{
    FromStart = 0,
    FromEnd = int.MaxValue,
}